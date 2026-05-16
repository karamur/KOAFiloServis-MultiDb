using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KOAFiloServis.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGuzergahKapasiteAdiAndSeferleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KapasiteAdi",
                table: "Guzergahlar",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GuzergahSeferleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GuzergahId = table.Column<int>(type: "integer", nullable: false),
                    Sira = table.Column<int>(type: "integer", nullable: false),
                    SeferTipi = table.Column<int>(type: "integer", nullable: false),
                    KapasiteAdi = table.Column<string>(type: "text", nullable: true),
                    AracId = table.Column<int>(type: "integer", nullable: true),
                    SoforAd = table.Column<string>(type: "text", nullable: true),
                    SoforTelefon = table.Column<string>(type: "text", nullable: true),
                    Firma = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuzergahSeferleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuzergahSeferleri_Araclar_AracId",
                        column: x => x.AracId,
                        principalTable: "Araclar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GuzergahSeferleri_Guzergahlar_GuzergahId",
                        column: x => x.GuzergahId,
                        principalTable: "Guzergahlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuzergahSeferleri_AracId",
                table: "GuzergahSeferleri",
                column: "AracId");

            migrationBuilder.CreateIndex(
                name: "IX_GuzergahSeferleri_GuzergahId",
                table: "GuzergahSeferleri",
                column: "GuzergahId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuzergahSeferleri");

            migrationBuilder.DropColumn(
                name: "KapasiteAdi",
                table: "Guzergahlar");
        }
    }
}
