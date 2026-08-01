using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MKFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyFieldsFromGuzergahSefer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuzergahSeferleri_Araclar_AracId",
                table: "GuzergahSeferleri");

            migrationBuilder.DropIndex(
                name: "IX_GuzergahSeferleri_AracId",
                table: "GuzergahSeferleri");

            migrationBuilder.DropColumn(
                name: "AracId",
                table: "GuzergahSeferleri");

            migrationBuilder.DropColumn(
                name: "SoforAd",
                table: "GuzergahSeferleri");

            migrationBuilder.DropColumn(
                name: "SoforTelefon",
                table: "GuzergahSeferleri");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AracId",
                table: "GuzergahSeferleri",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoforAd",
                table: "GuzergahSeferleri",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoforTelefon",
                table: "GuzergahSeferleri",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuzergahSeferleri_AracId",
                table: "GuzergahSeferleri",
                column: "AracId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuzergahSeferleri_Araclar_AracId",
                table: "GuzergahSeferleri",
                column: "AracId",
                principalTable: "Araclar",
                principalColumn: "Id");
        }
    }
}
