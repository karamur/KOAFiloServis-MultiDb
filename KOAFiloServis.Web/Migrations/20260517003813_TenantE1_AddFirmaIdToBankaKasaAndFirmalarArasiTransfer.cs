using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KOAFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class TenantE1_AddFirmaIdToBankaKasaAndFirmalarArasiTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FirmaId",
                table: "BankaKasaHareketleri",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FirmaId",
                table: "BankaHesaplari",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FirmalarArasiTransferler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KaynakFirmaId = table.Column<int>(type: "integer", nullable: false),
                    HedefFirmaId = table.Column<int>(type: "integer", nullable: false),
                    KaynakHesapId = table.Column<int>(type: "integer", nullable: false),
                    HedefHesapId = table.Column<int>(type: "integer", nullable: false),
                    Tutar = table.Column<decimal>(type: "numeric", nullable: false),
                    TransferTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BelgeNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    KaynakHareketId = table.Column<int>(type: "integer", nullable: true),
                    HedefHareketId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirmalarArasiTransferler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FirmalarArasiTransferler_BankaHesaplari_HedefHesapId",
                        column: x => x.HedefHesapId,
                        principalTable: "BankaHesaplari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FirmalarArasiTransferler_BankaHesaplari_KaynakHesapId",
                        column: x => x.KaynakHesapId,
                        principalTable: "BankaHesaplari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FirmalarArasiTransferler_BankaKasaHareketleri_HedefHareketId",
                        column: x => x.HedefHareketId,
                        principalTable: "BankaKasaHareketleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FirmalarArasiTransferler_BankaKasaHareketleri_KaynakHareket~",
                        column: x => x.KaynakHareketId,
                        principalTable: "BankaKasaHareketleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FirmalarArasiTransferler_Firmalar_HedefFirmaId",
                        column: x => x.HedefFirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FirmalarArasiTransferler_Firmalar_KaynakFirmaId",
                        column: x => x.KaynakFirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankaKasaHareketleri_FirmaId",
                table: "BankaKasaHareketleri",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_BankaHesaplari_FirmaId",
                table: "BankaHesaplari",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_FirmalarArasiTransferler_HedefFirmaId",
                table: "FirmalarArasiTransferler",
                column: "HedefFirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_FirmalarArasiTransferler_HedefHareketId",
                table: "FirmalarArasiTransferler",
                column: "HedefHareketId");

            migrationBuilder.CreateIndex(
                name: "IX_FirmalarArasiTransferler_HedefHesapId",
                table: "FirmalarArasiTransferler",
                column: "HedefHesapId");

            migrationBuilder.CreateIndex(
                name: "IX_FirmalarArasiTransferler_KaynakFirmaId",
                table: "FirmalarArasiTransferler",
                column: "KaynakFirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_FirmalarArasiTransferler_KaynakHareketId",
                table: "FirmalarArasiTransferler",
                column: "KaynakHareketId");

            migrationBuilder.CreateIndex(
                name: "IX_FirmalarArasiTransferler_KaynakHesapId",
                table: "FirmalarArasiTransferler",
                column: "KaynakHesapId");

            migrationBuilder.CreateIndex(
                name: "IX_FirmalarArasiTransferler_TransferTarihi",
                table: "FirmalarArasiTransferler",
                column: "TransferTarihi");

            migrationBuilder.AddForeignKey(
                name: "FK_BankaHesaplari_Firmalar_FirmaId",
                table: "BankaHesaplari",
                column: "FirmaId",
                principalTable: "Firmalar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BankaKasaHareketleri_Firmalar_FirmaId",
                table: "BankaKasaHareketleri",
                column: "FirmaId",
                principalTable: "Firmalar",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankaHesaplari_Firmalar_FirmaId",
                table: "BankaHesaplari");

            migrationBuilder.DropForeignKey(
                name: "FK_BankaKasaHareketleri_Firmalar_FirmaId",
                table: "BankaKasaHareketleri");

            migrationBuilder.DropTable(
                name: "FirmalarArasiTransferler");

            migrationBuilder.DropIndex(
                name: "IX_BankaKasaHareketleri_FirmaId",
                table: "BankaKasaHareketleri");

            migrationBuilder.DropIndex(
                name: "IX_BankaHesaplari_FirmaId",
                table: "BankaHesaplari");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "BankaKasaHareketleri");

            migrationBuilder.DropColumn(
                name: "FirmaId",
                table: "BankaHesaplari");
        }
    }
}
