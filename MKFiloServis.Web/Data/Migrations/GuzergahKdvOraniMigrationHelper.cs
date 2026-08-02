using MKFiloServis.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MKFiloServis.Web.Data.Migrations;

/// <summary>
/// Guzergah entity'sine KdvOrani kolonu eklemek için idempotent migration helper.
/// Multi-tenant deployment'da race-condition safe.
/// </summary>
public static class GuzergahKdvOraniMigrationHelper
{
    public static async Task ApplyAsync(DbContext context, ILogger logger)
    {
        if (context.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
        {
            // SQLite: PRAGMA ile kolon kontrolü
            var conn = context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
                await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA table_info(\"Guzergahlar\")";
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync()) cols.Add(reader.GetString(1));
            if (!cols.Contains("KdvOrani"))
            {
                using var addCmd = conn.CreateCommand();
                addCmd.CommandText = "ALTER TABLE \"Guzergahlar\" ADD COLUMN \"KdvOrani\" NUMERIC NOT NULL DEFAULT 20.0";
                await addCmd.ExecuteNonQueryAsync();
            }
            logger.LogInformation("GuzergahKdvOraniMigrationHelper: Guzergahlar.KdvOrani kolonu kontrol edildi/eklendi (SQLite)");
            return;
        }

        var sql = @"
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                               WHERE table_name = 'Guzergahlar' AND column_name = 'KdvOrani') THEN
                    ALTER TABLE ""Guzergahlar"" ADD COLUMN ""KdvOrani"" numeric(10,2) NOT NULL DEFAULT 20.0;
                END IF;
            END $$;
        ";

        await context.Database.ExecuteSqlRawAsync(sql);
        logger.LogInformation("GuzergahKdvOraniMigrationHelper: Guzergahlar.KdvOrani kolonu kontrol edildi/eklendi");
    }
}
