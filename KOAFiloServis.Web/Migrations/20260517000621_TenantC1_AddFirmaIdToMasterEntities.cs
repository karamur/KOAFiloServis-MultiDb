using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KOAFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class TenantC1_AddFirmaIdToMasterEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FirmaId",
                table: "Kurumlar",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FirmaId",
                table: "Araclar",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kurumlar_FirmaId",
                table: "Kurumlar",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Araclar_FirmaId",
                table: "Araclar",
                column: "FirmaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Araclar_Firmalar_FirmaId",
                table: "Araclar",
                column: "FirmaId",
                principalTable: "Firmalar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Kurumlar_Firmalar_FirmaId",
                table: "Kurumlar",
                column: "FirmaId",
                principalTable: "Firmalar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Araclar_Firmalar_FirmaId",
                table: "Araclar");

            migrationBuilder.DropForeignKey(
                name: "FK_Kurumlar_Firmalar_FirmaId",
                table: "Kurumlar");

            migrationBuilder.DropIndex(
                name: "IX_Kurumlar_FirmaId",
                table: "Kurumlar");

            migrationBuilder.DropIndex(
                name: "IX_Araclar_FirmaId",
                table: "Araclar");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "Kurumlar");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "Araclar");
        }
    }
}
