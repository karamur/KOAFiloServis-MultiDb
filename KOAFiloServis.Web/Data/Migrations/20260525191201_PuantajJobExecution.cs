using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KOAFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class PuantajJobExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PuantajJobExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirmaId = table.Column<int>(type: "integer", nullable: false),
                    Yil = table.Column<int>(type: "integer", nullable: false),
                    Ay = table.Column<int>(type: "integer", nullable: false),
                    KurumId = table.Column<int>(type: "integer", nullable: true),
                    Tetikleyen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Durum = table.Column<int>(type: "integer", nullable: false),
                    Baslangic = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Bitis = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Versiyon = table.Column<int>(type: "integer", nullable: false),
                    HesapDonemiId = table.Column<int>(type: "integer", nullable: true),
                    IslenenOperasyon = table.Column<int>(type: "integer", nullable: false),
                    UretilenPuantaj = table.Column<int>(type: "integer", nullable: false),
                    HataMesaji = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Hesaplayan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuantajJobExecutions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuantajJobExecutions_Durum",
                table: "PuantajJobExecutions",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_PuantajJobExecutions_FirmaId_Yil_Ay",
                table: "PuantajJobExecutions",
                columns: new[] { "FirmaId", "Yil", "Ay" },
                unique: true,
                filter: "\"Durum\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PuantajJobExecutions");
        }
    }
}
