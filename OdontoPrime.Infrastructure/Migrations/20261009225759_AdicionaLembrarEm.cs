using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdontoPrime.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaLembrarEm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LembrarEm",
                table: "Consultas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_LembretesPendentes",
                table: "Consultas",
                column: "LembrarEm",
                filter: "\"LembreteEnviadoEm\" IS NULL AND \"LembrarEm\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consultas_LembretesPendentes",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "LembrarEm",
                table: "Consultas");
        }
    }
}
