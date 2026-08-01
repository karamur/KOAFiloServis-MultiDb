using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MKFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class OperasyonPuantajCekirdek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OperasyonKontratlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KurumCariId = table.Column<int>(type: "INTEGER", nullable: false),
                    KontratAdi = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubeId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperasyonKontratlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperasyonKontratlar_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperasyonKontratlar_Subeler_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Subeler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OperasyonPlanSatirlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FiloGuzergahEslestirmeId = table.Column<int>(type: "INTEGER", nullable: false),
                    KurumFirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuzergahId = table.Column<int>(type: "INTEGER", nullable: false),
                    AracId = table.Column<int>(type: "INTEGER", nullable: false),
                    SoforId = table.Column<int>(type: "INTEGER", nullable: false),
                    ServisTuru = table.Column<int>(type: "INTEGER", nullable: false),
                    PlanlananSefer = table.Column<decimal>(type: "TEXT", nullable: false),
                    PuantajCarpani = table.Column<decimal>(type: "TEXT", nullable: false),
                    KurumSeferUcretiSnapshot = table.Column<decimal>(type: "TEXT", nullable: false),
                    TaseronSeferUcretiSnapshot = table.Column<decimal>(type: "TEXT", nullable: false),
                    Durum = table.Column<int>(type: "INTEGER", nullable: false),
                    FiloGunlukPuantajId = table.Column<int>(type: "INTEGER", nullable: true),
                    TeyitTarihi = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notlar = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubeId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperasyonPlanSatirlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperasyonPlanSatirlari_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperasyonPlanSatirlari_Subeler_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Subeler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OperasyonTakvimGunleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tarih = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GunTipi = table.Column<int>(type: "INTEGER", nullable: false),
                    PuantajCarpani = table.Column<decimal>(type: "TEXT", nullable: false),
                    Aciklama = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubeId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperasyonTakvimGunleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperasyonTakvimGunleri_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperasyonTakvimGunleri_Subeler_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Subeler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OperasyonKontratFiyatlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OperasyonKontratId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuzergahId = table.Column<int>(type: "INTEGER", nullable: false),
                    KurumSeferUcreti = table.Column<decimal>(type: "TEXT", nullable: false),
                    TaseronSeferUcreti = table.Column<decimal>(type: "TEXT", nullable: false),
                    GecerlilikBaslangic = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GecerlilikBitis = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedBy = table.Column<int>(type: "INTEGER", nullable: true),
                    FirmaId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubeId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedByUserId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperasyonKontratFiyatlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperasyonKontratFiyatlar_Firmalar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Firmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperasyonKontratFiyatlar_OperasyonKontratlar_OperasyonKontratId",
                        column: x => x.OperasyonKontratId,
                        principalTable: "OperasyonKontratlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperasyonKontratFiyatlar_Subeler_SubeId",
                        column: x => x.SubeId,
                        principalTable: "Subeler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonKontratFiyatlar_FirmaId",
                table: "OperasyonKontratFiyatlar",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonKontratFiyatlar_OperasyonKontratId_GuzergahId",
                table: "OperasyonKontratFiyatlar",
                columns: new[] { "OperasyonKontratId", "GuzergahId" });

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonKontratFiyatlar_SubeId",
                table: "OperasyonKontratFiyatlar",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonKontratlar_FirmaId",
                table: "OperasyonKontratlar",
                column: "FirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonKontratlar_SubeId",
                table: "OperasyonKontratlar",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonPlanSatirlari_FirmaId_Tarih_FiloGuzergahEslestirmeId_IsDeleted",
                table: "OperasyonPlanSatirlari",
                columns: new[] { "FirmaId", "Tarih", "FiloGuzergahEslestirmeId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonPlanSatirlari_SubeId",
                table: "OperasyonPlanSatirlari",
                column: "SubeId");

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonTakvimGunleri_FirmaId_Tarih_IsDeleted",
                table: "OperasyonTakvimGunleri",
                columns: new[] { "FirmaId", "Tarih", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_OperasyonTakvimGunleri_SubeId",
                table: "OperasyonTakvimGunleri",
                column: "SubeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperasyonKontratFiyatlar");

            migrationBuilder.DropTable(
                name: "OperasyonPlanSatirlari");

            migrationBuilder.DropTable(
                name: "OperasyonTakvimGunleri");

            migrationBuilder.DropTable(
                name: "OperasyonKontratlar");
        }
    }
}
