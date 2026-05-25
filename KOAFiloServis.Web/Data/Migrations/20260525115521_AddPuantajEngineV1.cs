using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KOAFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPuantajEngineV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OperasyonKayitlari_PuantajKayitlar_PuantajKayitId",
                table: "OperasyonKayitlari");

            migrationBuilder.DropIndex(
                name: "IX_OperasyonKayitlari_Islendi",
                table: "OperasyonKayitlari");

            migrationBuilder.DropIndex(
                name: "IX_OperasyonKayitlari_PuantajKayitId",
                table: "OperasyonKayitlari");

            migrationBuilder.DropColumn(
                name: "Islendi",
                table: "OperasyonKayitlari");

            migrationBuilder.DropColumn(
                name: "IslenmeTarihi",
                table: "OperasyonKayitlari");

            migrationBuilder.DropColumn(
                name: "PuantajKayitId",
                table: "OperasyonKayitlari");

            migrationBuilder.AddColumn<int>(
                name: "HesapDonemiId",
                table: "PuantajKayitlar",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OncekiVersiyonId",
                table: "PuantajKayitlar",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Versiyon",
                table: "PuantajKayitlar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PuantajHesapDonemleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirmaId = table.Column<int>(type: "integer", nullable: false),
                    Yil = table.Column<int>(type: "integer", nullable: false),
                    Ay = table.Column<int>(type: "integer", nullable: false),
                    KurumId = table.Column<int>(type: "integer", nullable: true),
                    Versiyon = table.Column<int>(type: "integer", nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    HesaplayanKullanici = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HesaplamaTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OncekiDonemId = table.Column<int>(type: "integer", nullable: true),
                    Notlar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuantajHesapDonemleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PuantajHesapDonemleri_PuantajHesapDonemleri_OncekiDonemId",
                        column: x => x.OncekiDonemId,
                        principalTable: "PuantajHesapDonemleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PuantajDetaylari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirmaId = table.Column<int>(type: "integer", nullable: false),
                    OperasyonKaydiId = table.Column<int>(type: "integer", nullable: false),
                    PuantajKayitId = table.Column<int>(type: "integer", nullable: false),
                    HesapDonemiId = table.Column<int>(type: "integer", nullable: false),
                    BirimGelir = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BirimGider = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SeferSayisi = table.Column<int>(type: "integer", nullable: false),
                    HesaplananTutar = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuantajDetaylari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PuantajDetaylari_OperasyonKayitlari_OperasyonKaydiId",
                        column: x => x.OperasyonKaydiId,
                        principalTable: "OperasyonKayitlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PuantajDetaylari_PuantajHesapDonemleri_HesapDonemiId",
                        column: x => x.HesapDonemiId,
                        principalTable: "PuantajHesapDonemleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PuantajDetaylari_PuantajKayitlar_PuantajKayitId",
                        column: x => x.PuantajKayitId,
                        principalTable: "PuantajKayitlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuantajKayitlar_HesapDonemiId",
                table: "PuantajKayitlar",
                column: "HesapDonemiId");

            migrationBuilder.CreateIndex(
                name: "IX_PuantajKayitlar_OncekiVersiyonId",
                table: "PuantajKayitlar",
                column: "OncekiVersiyonId");

            migrationBuilder.CreateIndex(
                name: "IX_PuantajDetaylari_HesapDonemiId",
                table: "PuantajDetaylari",
                column: "HesapDonemiId");

            migrationBuilder.CreateIndex(
                name: "IX_PuantajDetaylari_OperasyonKaydiId_HesapDonemiId",
                table: "PuantajDetaylari",
                columns: new[] { "OperasyonKaydiId", "HesapDonemiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PuantajDetaylari_PuantajKayitId",
                table: "PuantajDetaylari",
                column: "PuantajKayitId");

            migrationBuilder.CreateIndex(
                name: "IX_PuantajHesapDonemleri_Durum",
                table: "PuantajHesapDonemleri",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_PuantajHesapDonemleri_FirmaId_Yil_Ay_KurumId",
                table: "PuantajHesapDonemleri",
                columns: new[] { "FirmaId", "Yil", "Ay", "KurumId" });

            migrationBuilder.CreateIndex(
                name: "IX_PuantajHesapDonemleri_FirmaId_Yil_Ay_KurumId_Versiyon",
                table: "PuantajHesapDonemleri",
                columns: new[] { "FirmaId", "Yil", "Ay", "KurumId", "Versiyon" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PuantajHesapDonemleri_OncekiDonemId",
                table: "PuantajHesapDonemleri",
                column: "OncekiDonemId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PuantajKayitlar_PuantajHesapDonemleri_HesapDonemiId",
                table: "PuantajKayitlar");

            migrationBuilder.DropForeignKey(
                name: "FK_PuantajKayitlar_PuantajKayitlar_OncekiVersiyonId",
                table: "PuantajKayitlar");

            migrationBuilder.DropTable(
                name: "PuantajDetaylari");

            migrationBuilder.DropTable(
                name: "PuantajHesapDonemleri");

            migrationBuilder.DropIndex(
                name: "IX_PuantajKayitlar_HesapDonemiId",
                table: "PuantajKayitlar");

            migrationBuilder.DropIndex(
                name: "IX_PuantajKayitlar_OncekiVersiyonId",
                table: "PuantajKayitlar");

            migrationBuilder.DropColumn(
                name: "HesapDonemiId",
                table: "PuantajKayitlar");

            migrationBuilder.DropColumn(
                name: "OncekiVersiyonId",
                table: "PuantajKayitlar");

            migrationBuilder.DropColumn(
                name: "Versiyon",
                table: "PuantajKayitlar");

            migrationBuilder.AddColumn<bool>(
                name: "Islendi",
                table: "OperasyonKayitlari",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "IslenmeTarihi",
                table: "OperasyonKayitlari",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PuantajKayitId",
                table: "OperasyonKayitlari",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonKayitlari_Islendi",
                table: "OperasyonKayitlari",
                column: "Islendi");

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonKayitlari_PuantajKayitId",
                table: "OperasyonKayitlari",
                column: "PuantajKayitId");

            migrationBuilder.AddForeignKey(
                name: "FK_OperasyonKayitlari_PuantajKayitlar_PuantajKayitId",
                table: "OperasyonKayitlari",
                column: "PuantajKayitId",
                principalTable: "PuantajKayitlar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
