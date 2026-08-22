#:package Microsoft.Data.Sqlite@10.0.5

using Microsoft.Data.Sqlite;

var con = new SqliteConnection("Data Source=MKFiloServis.Web/MKFiloServis.db;Mode=ReadOnly");
con.Open();
var cmd = con.CreateCommand();
cmd.CommandText = "SELECT Id, FirmaAdi, IsDeleted FROM Firmalar; SELECT Id, SaseNo, AktifPlaka, FirmaId, KaynakFirmaId, KaynakKayitId, IsDeleted, UpdatedAt FROM Araclar WHERE SaseNo='W1V4KDGZ1RP669771'";
using var r = cmd.ExecuteReader();
do
{
    while (r.Read())
    {
        for (int i = 0; i < r.FieldCount; i++)
            Console.Write($"{r.GetName(i)}={r.GetValue(i)} | ");
        Console.WriteLine();
    }
    Console.WriteLine("---");
} while (r.NextResult());
