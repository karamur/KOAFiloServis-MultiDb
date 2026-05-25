using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KOAFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOnayWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FinansOnayTarihi",
                table: "PuantajHesapDonemleri",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinansOnaylayan",
                table: "PuantajHesapDonemleri",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KilitAciklama",
                table: "PuantajHesapDonemleri",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "KilitTarihi",
                table: "PuantajHesapDonemleri",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MuhasebeOnayTarihi",
                table: "PuantajHesapDonemleri",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MuhasebeOnaylayan",
                table: "PuantajHesapDonemleri",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OnayDurum",
                table: "PuantajHesapDonemleri",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PuantajAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirmaId = table.Column<int>(type: "integer", nullable: true),
                    HesapDonemiId = table.Column<int>(type: "integer", nullable: true),
                    Aksiyon = table.Column<int>(type: "integer", nullable: false),
                    Kullanici = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AksiyonTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OncekiDurum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    YeniDurum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuantajAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuantajAuditLogs_FirmaId",
                table: "PuantajAuditLogs",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_PuantajAuditLogs_HesapDonemiId_AksiyonTarihi",
                table: "PuantajAuditLogs",
                columns: new[] { "HesapDonemiId", "AksiyonTarihi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PuantajAuditLogs");

            migrationBuilder.DropColumn(
                name: "FinansOnayTarihi",
                table: "PuantajHesapDonemleri");

            migrationBuilder.DropColumn(
                name: "FinansOnaylayan",
                table: "PuantajHesapDonemleri");

            migrationBuilder.DropColumn(
                name: "KilitAciklama",
                table: "PuantajHesapDonemleri");

            migrationBuilder.DropColumn(
                name: "KilitTarihi",
                table: "PuantajHesapDonemleri");

            migrationBuilder.DropColumn(
                name: "MuhasebeOnayTarihi",
                table: "PuantajHesapDonemleri");

            migrationBuilder.DropColumn(
                name: "MuhasebeOnaylayan",
                table: "PuantajHesapDonemleri");

            migrationBuilder.DropColumn(
                name: "OnayDurum",
                table: "PuantajHesapDonemleri");
        }
    }
}
