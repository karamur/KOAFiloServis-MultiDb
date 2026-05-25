using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KOAFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPuantajFinansalKayit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PuantajFinansalKayitlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirmaId = table.Column<int>(type: "integer", nullable: false),
                    PuantajKayitId = table.Column<int>(type: "integer", nullable: false),
                    HesapDonemiId = table.Column<int>(type: "integer", nullable: false),
                    BirimGelir = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BirimGider = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ToplamGelir = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ToplamGider = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    KdvTutar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GenelToplam = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SeferGunu = table.Column<int>(type: "integer", nullable: false),
                    GelirCariId = table.Column<int>(type: "integer", nullable: true),
                    GiderCariId = table.Column<int>(type: "integer", nullable: true),
                    GelirFaturaId = table.Column<int>(type: "integer", nullable: true),
                    GiderFaturaId = table.Column<int>(type: "integer", nullable: true),
                    KayitTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuantajFinansalKayitlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PuantajFinansalKayitlar_PuantajHesapDonemleri_HesapDonemiId",
                        column: x => x.HesapDonemiId,
                        principalTable: "PuantajHesapDonemleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PuantajFinansalKayitlar_PuantajKayitlar_PuantajKayitId",
                        column: x => x.PuantajKayitId,
                        principalTable: "PuantajKayitlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuantajFinansalKayitlar_Durum",
                table: "PuantajFinansalKayitlar",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_PuantajFinansalKayitlar_HesapDonemiId",
                table: "PuantajFinansalKayitlar",
                column: "HesapDonemiId");

            migrationBuilder.CreateIndex(
                name: "IX_PuantajFinansalKayitlar_PuantajKayitId_HesapDonemiId",
                table: "PuantajFinansalKayitlar",
                columns: new[] { "PuantajKayitId", "HesapDonemiId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PuantajFinansalKayitlar");
        }
    }
}
