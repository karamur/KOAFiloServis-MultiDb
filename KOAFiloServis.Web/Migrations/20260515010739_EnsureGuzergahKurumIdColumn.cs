using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KOAFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnsureGuzergahKurumIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Guzergahlar"" ADD COLUMN IF NOT EXISTS ""KurumId"" integer NULL;
                CREATE INDEX IF NOT EXISTS ""IX_Guzergahlar_KurumId"" ON ""Guzergahlar"" (""KurumId"");
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_Guzergahlar_Kurumlar_KurumId'
                          AND table_name = 'Guzergahlar'
                    ) THEN
                        ALTER TABLE ""Guzergahlar""
                            ADD CONSTRAINT ""FK_Guzergahlar_Kurumlar_KurumId""
                            FOREIGN KEY (""KurumId"") REFERENCES ""Kurumlar"" (""Id"") ON DELETE NO ACTION;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""Guzergahlar"" DROP CONSTRAINT IF EXISTS ""FK_Guzergahlar_Kurumlar_KurumId"";
                DROP INDEX IF EXISTS ""IX_Guzergahlar_KurumId"";
                ALTER TABLE ""Guzergahlar"" DROP COLUMN IF EXISTS ""KurumId"";
            ");
        }
    }
}
