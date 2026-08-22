using Microsoft.Data.Sqlite;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKFiloServis.DataSync.Exporters;

/// <summary>
/// SQLite veritabanindaki tum tablolari okuyup PostgreSQL hedefine aktarir.
/// Sema hedef PostgreSQL'de zaten olmalidir. Bu sinif sadece VERI kopyalar.
/// FK bakimindan session_replication_role=replica ile tek transaction kullanir,
/// aktarim sonrasi identity sequence'lari MAX(Id)'ye gore resetler.
/// </summary>
public sealed class SqliteToPostgresImporter
{
    private readonly string _sqlitePath;
    private readonly string _pgConnectionString;
    private readonly Action<string> _progress;

    public SqliteToPostgresImporter(string sqlitePath, string pgConnectionString, Action<string>? progress = null)
    {
        _sqlitePath = sqlitePath;
        _pgConnectionString = pgConnectionString;
        _progress = progress ?? (_ => { });
    }

    public async Task RunAsync()
    {
        if (!File.Exists(_sqlitePath))
            throw new FileNotFoundException($"Kaynak SQLite veritabani bulunamadi: {_sqlitePath}");

        _progress($"▸ Kaynak: {_sqlitePath}");
        _progress($"▸ Hedef : PostgreSQL");

        var sqliteConnString = new SqliteConnectionStringBuilder { DataSource = _sqlitePath, Mode = SqliteOpenMode.ReadOnly }.ToString();
        await using var sqlite = new SqliteConnection(sqliteConnString);
        await sqlite.OpenAsync();

        await using var pg = new NpgsqlConnection(_pgConnectionString);
        await pg.OpenAsync();

        var sqliteTables = await ListSqliteUserTablesAsync(sqlite);
        _progress($"▸ Kaynak SQLite'da {sqliteTables.Count} tablo tespit edildi.");

        var pgTables = await ListPostgresUserTablesAsync(pg);
        _progress($"▸ Hedef PG'de {pgTables.Count} tablo tespit edildi.");

        var ortakTablolar = sqliteTables
            .Where(t => pgTables.Contains(t, StringComparer.OrdinalIgnoreCase))
            .ToList();
        _progress($"▸ Kopyalanacak tablo sayisi: {ortakTablolar.Count}");

        var atlanan = sqliteTables.Except(ortakTablolar, StringComparer.OrdinalIgnoreCase).ToList();
        foreach (var t in atlanan)
            _progress($"  ⚠ PG'de karsiligi yok, atlaniyor: {t}");

        await using var tx = await pg.BeginTransactionAsync();

        // FK tetikleyicilerini bu oturum icin devre disi birak
        await using (var replica = pg.CreateCommand())
        {
            replica.Transaction = tx;
            replica.CommandText = "SET session_replication_role = replica;";
            await replica.ExecuteNonQueryAsync();
        }

        var sonuclar = new List<(string Tablo, long Kaynak, long Kopyalanan)>();
        int tabloIndex = 0;
        long toplamSatir = 0;

        foreach (var tablo in ortakTablolar)
        {
            tabloIndex++;
            var pgTabloAdi = pgTables.First(t => t.Equals(tablo, StringComparison.OrdinalIgnoreCase));

            await using (var del = pg.CreateCommand())
            {
                del.Transaction = tx;
                del.CommandText = $"DELETE FROM public.\"{pgTabloAdi}\";";
                await del.ExecuteNonQueryAsync();
            }

            var (kaynakSatir, kopyalanan) = await KopyalaTabloAsync(sqlite, pg, tx, tablo, pgTabloAdi);
            toplamSatir += kopyalanan;
            sonuclar.Add((pgTabloAdi, kaynakSatir, kopyalanan));
            _progress($"  [{tabloIndex}/{ortakTablolar.Count}] {pgTabloAdi}: {kopyalanan}/{kaynakSatir} satir");

            if (kaynakSatir != kopyalanan)
                throw new InvalidOperationException($"Satir sayisi uyusmuyor: {pgTabloAdi} kaynak={kaynakSatir}, kopyalanan={kopyalanan}");
        }

        await tx.CommitAsync();

        // Identity/sequence reset
        await ResetSequencesAsync(pg, sonuclar.Select(s => s.Tablo).ToList());

        _progress($"✔ Toplam {toplamSatir} satir kopyalandi.");
    }

    private static async Task<List<string>> ListSqliteUserTablesAsync(SqliteConnection conn)
    {
        var list = new List<string>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE '__EFMigrations%' ORDER BY name;";
        await using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync()) list.Add(rdr.GetString(0));
        return list;
    }

    private static async Task<List<string>> ListPostgresUserTablesAsync(NpgsqlConnection conn)
    {
        var list = new List<string>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT table_name FROM information_schema.tables
                            WHERE table_schema = 'public' AND table_type = 'BASE TABLE'
                              AND table_name NOT LIKE '__EFMigrations%'
                            ORDER BY table_name;";
        await using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync()) list.Add(rdr.GetString(0));
        return list;
    }

    private static async Task<List<string>> ListSqliteColumnsAsync(SqliteConnection conn, string tablo)
    {
        var list = new List<string>();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"PRAGMA table_info(\"{tablo}\");";
        await using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync()) list.Add(rdr.GetString(1));
        return list;
    }

    private sealed record PgColumn(string Name, string DataType, bool IsNullable);

    private static async Task<List<PgColumn>> ListPgColumnsAsync(NpgsqlConnection conn, NpgsqlTransaction tx, string tablo)
    {
        var list = new List<PgColumn>();
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = @"SELECT column_name, data_type, is_nullable FROM information_schema.columns
                            WHERE table_schema = 'public' AND table_name = @t
                            ORDER BY ordinal_position;";
        cmd.Parameters.AddWithValue("t", tablo);
        await using var rdr = await cmd.ExecuteReaderAsync();
        while (await rdr.ReadAsync())
            list.Add(new PgColumn(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2) == "YES"));
        return list;
    }

    private async Task<(long Kaynak, long Kopyalanan)> KopyalaTabloAsync(
        SqliteConnection sqlite, NpgsqlConnection pg, NpgsqlTransaction tx, string sqliteTablo, string pgTablo)
    {
        var sqliteKolonlar = await ListSqliteColumnsAsync(sqlite, sqliteTablo);
        var pgKolonlar = await ListPgColumnsAsync(pg, tx, pgTablo);

        var ortak = pgKolonlar
            .Where(p => sqliteKolonlar.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();
        if (ortak.Count == 0) return (0, 0);

        // Kaynak satir sayisi
        long kaynakSatir;
        await using (var cnt = sqlite.CreateCommand())
        {
            cnt.CommandText = $"SELECT COUNT(*) FROM \"{sqliteTablo}\";";
            kaynakSatir = Convert.ToInt64(await cnt.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
        }
        if (kaynakSatir == 0) return (0, 0);

        var sqliteSelectKolonlari = ortak
            .Select(p => sqliteKolonlar.First(s => s.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var select = "SELECT " + string.Join(", ", sqliteSelectKolonlari.Select(c => $"\"{c}\"")) + $" FROM \"{sqliteTablo}\";";

        await using var srcCmd = sqlite.CreateCommand();
        srcCmd.CommandText = select;
        await using var rdr = await srcCmd.ExecuteReaderAsync();

        var copyCmd = $"COPY public.\"{pgTablo}\" ({string.Join(", ", ortak.Select(c => $"\"{c.Name}\""))}) FROM STDIN (FORMAT BINARY)";
        long kopyalanan = 0;

        await using (var writer = await pg.BeginBinaryImportAsync(copyCmd))
        {
            while (await rdr.ReadAsync())
            {
                await writer.StartRowAsync();
                for (int i = 0; i < ortak.Count; i++)
                {
                    var deger = rdr.IsDBNull(i) ? null : rdr.GetValue(i);
                    await YazDegerAsync(writer, deger, ortak[i].DataType);
                }
                kopyalanan++;
            }
            await writer.CompleteAsync();
        }

        return (kaynakSatir, kopyalanan);
    }

    private static async Task YazDegerAsync(NpgsqlBinaryImporter writer, object? deger, string pgDataType)
    {
        if (deger is null || deger is DBNull)
        {
            await writer.WriteNullAsync();
            return;
        }

        switch (pgDataType)
        {
            case "boolean":
                await writer.WriteAsync(ToBool(deger), NpgsqlDbType.Boolean);
                break;
            case "smallint":
                await writer.WriteAsync(Convert.ToInt16(deger, CultureInfo.InvariantCulture), NpgsqlDbType.Smallint);
                break;
            case "integer":
                await writer.WriteAsync(Convert.ToInt32(deger, CultureInfo.InvariantCulture), NpgsqlDbType.Integer);
                break;
            case "bigint":
                await writer.WriteAsync(Convert.ToInt64(deger, CultureInfo.InvariantCulture), NpgsqlDbType.Bigint);
                break;
            case "numeric":
                await writer.WriteAsync(ToDecimal(deger), NpgsqlDbType.Numeric);
                break;
            case "real":
                await writer.WriteAsync(Convert.ToSingle(deger, CultureInfo.InvariantCulture), NpgsqlDbType.Real);
                break;
            case "double precision":
                await writer.WriteAsync(Convert.ToDouble(deger, CultureInfo.InvariantCulture), NpgsqlDbType.Double);
                break;
            case "uuid":
                await writer.WriteAsync(Guid.Parse(Convert.ToString(deger, CultureInfo.InvariantCulture)!), NpgsqlDbType.Uuid);
                break;
            case "timestamp with time zone":
                await writer.WriteAsync(ToDateTime(deger), NpgsqlDbType.TimestampTz);
                break;
            case "timestamp without time zone":
                await writer.WriteAsync(ToDateTime(deger), NpgsqlDbType.Timestamp);
                break;
            case "date":
                await writer.WriteAsync(ToDateTime(deger).Date, NpgsqlDbType.Date);
                break;
            case "time without time zone":
                await writer.WriteAsync(ToTimeSpan(deger), NpgsqlDbType.Time);
                break;
            case "interval":
                await writer.WriteAsync(ToTimeSpan(deger), NpgsqlDbType.Interval);
                break;
            case "bytea":
                await writer.WriteAsync((byte[])deger, NpgsqlDbType.Bytea);
                break;
            case "jsonb":
                await writer.WriteAsync(Convert.ToString(deger, CultureInfo.InvariantCulture)!, NpgsqlDbType.Jsonb);
                break;
            case "json":
                await writer.WriteAsync(Convert.ToString(deger, CultureInfo.InvariantCulture)!, NpgsqlDbType.Json);
                break;
            default: // text, character varying vs.
                await writer.WriteAsync(Convert.ToString(deger, CultureInfo.InvariantCulture)!, NpgsqlDbType.Text);
                break;
        }
    }

    private static bool ToBool(object deger) => deger switch
    {
        bool b => b,
        long l => l != 0,
        int i => i != 0,
        string s => s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase),
        _ => Convert.ToInt64(deger, CultureInfo.InvariantCulture) != 0
    };

    private static decimal ToDecimal(object deger) => deger switch
    {
        decimal d => d,
        string s => decimal.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture),
        _ => Convert.ToDecimal(deger, CultureInfo.InvariantCulture)
    };

    private static DateTime ToDateTime(object deger) => deger switch
    {
        DateTime dt => dt,
        string s => DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.None),
        long l => DateTime.FromBinary(l),
        _ => Convert.ToDateTime(deger, CultureInfo.InvariantCulture)
    };

    private static TimeSpan ToTimeSpan(object deger) => deger switch
    {
        TimeSpan ts => ts,
        string s => TimeSpan.Parse(s, CultureInfo.InvariantCulture),
        long l => TimeSpan.FromTicks(l),
        _ => TimeSpan.Zero
    };

    private async Task ResetSequencesAsync(NpgsqlConnection pg, List<string> tablolar)
    {
        _progress("▸ Sequence'lar resetleniyor...");
        foreach (var tablo in tablolar)
        {
            await using var cmd = pg.CreateCommand();
            cmd.CommandText = @"SELECT column_name FROM information_schema.columns
                                WHERE table_schema='public' AND table_name=@t
                                  AND (column_default LIKE 'nextval%' OR is_identity='YES');";
            cmd.Parameters.AddWithValue("t", tablo);

            var idKolonlari = new List<string>();
            await using (var rdr = await cmd.ExecuteReaderAsync())
            {
                while (await rdr.ReadAsync()) idKolonlari.Add(rdr.GetString(0));
            }

            foreach (var kolon in idKolonlari)
            {
                await using var reset = pg.CreateCommand();
                reset.CommandText = $@"SELECT setval(
                    pg_get_serial_sequence('public.""{tablo}""', '{kolon}'),
                    GREATEST(COALESCE((SELECT MAX(""{kolon}"") FROM public.""{tablo}""), 0), 1),
                    (SELECT COUNT(*) > 0 FROM public.""{tablo}""));";
                await reset.ExecuteNonQueryAsync();
            }
        }
        _progress("▸ Sequence reset tamamlandi.");
    }
}
