using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KOAFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPuantajOnayKolonlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OnayTarihi",
                table: "FiloGunlukPuantajlar",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Onaylandi",
                table: "FiloGunlukPuantajlar",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnayTarihi",
                table: "FiloGunlukPuantajlar");

            migrationBuilder.DropColumn(
                name: "Onaylandi",
                table: "FiloGunlukPuantajlar");
        }
    }
}
