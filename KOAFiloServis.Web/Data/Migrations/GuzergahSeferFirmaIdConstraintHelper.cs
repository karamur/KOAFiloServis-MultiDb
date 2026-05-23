using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KOAFiloServis.Web.Data.Migrations;

/// <summary>
/// GuzergahSeferleri tablosundaki FK_GuzergahSeferleri_Firmalar_FirmaId
/// constraint'ini kaldirir. Seferler firmaya guzergah uzerinden baglidir,
/// ayrica FirmaId gereksiz ve tenant DB'lerde FK ihlaline yol acar.
/// </summary>
public static class GuzergahSeferFirmaIdConstraintHelper
{
    public static async Task ApplyAsync(ApplicationDbContext context, ILogger? logger = null)
    {
        try
        {
            var sql = """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_GuzergahSeferleri_Firmalar_FirmaId'
                        AND table_name = 'GuzergahSeferleri'
                    ) THEN
                        ALTER TABLE "GuzergahSeferleri" DROP CONSTRAINT "FK_GuzergahSeferleri_Firmalar_FirmaId";
                    END IF;
                END $$;
                """;
            await context.Database.ExecuteSqlRawAsync(sql);
            logger?.LogInformation("GuzergahSeferFirmaIdConstraint: FK constraint kaldirildi.");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "GuzergahSeferFirmaIdConstraint: FK kaldirilirken hata (kritik degil)");
        }
    }
}
