using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KOAFiloServis.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddPuantajCarpaniToGuzergah : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PuantajKayitlar_PuantajHesapDonemleri_HesapDonemiId",
                table: "PuantajKayitlar");

            migrationBuilder.DropForeignKey(
                name: "FK_PuantajKayitlar_PuantajKayitlar_OncekiVersiyonId",
                table: "PuantajKayitlar");

            migrationBuilder.DropIndex(
                name: "IX_PuantajKayitlar_Yil_Ay_GuzergahId_AracId_Slot",
                table: "PuantajKayitlar");

            migrationBuilder.AddColumn<decimal>(
                name: "PuantajCarpani",
                table: "Guzergahlar",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 1.0m);

            migrationBuilder.CreateIndex(
                name: "IX_PuantajKayitlar_Yil_Ay_GuzergahId_AracId_Slot",
                table: "PuantajKayitlar",
                columns: new[] { "Yil", "Ay", "GuzergahId", "AracId", "Slot" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_PuantajKayitlar_PuantajHesapDonemleri_HesapDonemiId",
                table: "PuantajKayitlar",
                column: "HesapDonemiId",
                principalTable: "PuantajHesapDonemleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PuantajKayitlar_PuantajKayitlar_OncekiVersiyonId",
                table: "PuantajKayitlar",
                column: "OncekiVersiyonId",
                principalTable: "PuantajKayitlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PuantajKayitlar_PuantajHesapDonemleri_HesapDonemiId",
                table: "PuantajKayitlar");

            migrationBuilder.DropForeignKey(
                name: "FK_PuantajKayitlar_PuantajKayitlar_OncekiVersiyonId",
                table: "PuantajKayitlar");

            migrationBuilder.DropIndex(
                name: "IX_PuantajKayitlar_Yil_Ay_GuzergahId_AracId_Slot",
                table: "PuantajKayitlar");

            migrationBuilder.DropColumn(
                name: "PuantajCarpani",
                table: "Guzergahlar");

            migrationBuilder.CreateIndex(
                name: "IX_PuantajKayitlar_Yil_Ay_GuzergahId_AracId_Slot",
                table: "PuantajKayitlar",
                columns: new[] { "Yil", "Ay", "GuzergahId", "AracId", "Slot" });

            migrationBuilder.AddForeignKey(
                name: "FK_PuantajKayitlar_PuantajHesapDonemleri_HesapDonemiId",
                table: "PuantajKayitlar",
                column: "HesapDonemiId",
                principalTable: "PuantajHesapDonemleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PuantajKayitlar_PuantajKayitlar_OncekiVersiyonId",
                table: "PuantajKayitlar",
                column: "OncekiVersiyonId",
                principalTable: "PuantajKayitlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
