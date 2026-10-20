using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtikKurul.Api.Migrations
{
    /// <inheritdoc />
    public partial class EtikKurulOylamasi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Kararlar_KararNo",
                table: "Kararlar");

            migrationBuilder.AlterColumn<string>(
                name: "KararVerenKullaniciId",
                table: "Kararlar",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "KararNo",
                table: "Kararlar",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "KabulOySayisi",
                table: "Kararlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "OyBirligiMi",
                table: "Kararlar",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RedOySayisi",
                table: "Kararlar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EtikKurulOylari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasvuruId = table.Column<int>(type: "int", nullable: false),
                    UyeKullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Onayladi = table.Column<bool>(type: "bit", nullable: true),
                    GonderimTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OyTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtikKurulOylari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtikKurulOylari_AspNetUsers_UyeKullaniciId",
                        column: x => x.UyeKullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EtikKurulOylari_Basvurular_BasvuruId",
                        column: x => x.BasvuruId,
                        principalTable: "Basvurular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kararlar_KararNo",
                table: "Kararlar",
                column: "KararNo",
                unique: true,
                filter: "[KararNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EtikKurulOylari_BasvuruId_UyeKullaniciId",
                table: "EtikKurulOylari",
                columns: new[] { "BasvuruId", "UyeKullaniciId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtikKurulOylari_UyeKullaniciId",
                table: "EtikKurulOylari",
                column: "UyeKullaniciId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EtikKurulOylari");

            migrationBuilder.DropIndex(
                name: "IX_Kararlar_KararNo",
                table: "Kararlar");

            migrationBuilder.DropColumn(
                name: "KabulOySayisi",
                table: "Kararlar");

            migrationBuilder.DropColumn(
                name: "OyBirligiMi",
                table: "Kararlar");

            migrationBuilder.DropColumn(
                name: "RedOySayisi",
                table: "Kararlar");

            migrationBuilder.AlterColumn<string>(
                name: "KararVerenKullaniciId",
                table: "Kararlar",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "KararNo",
                table: "Kararlar",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kararlar_KararNo",
                table: "Kararlar",
                column: "KararNo",
                unique: true);
        }
    }
}
