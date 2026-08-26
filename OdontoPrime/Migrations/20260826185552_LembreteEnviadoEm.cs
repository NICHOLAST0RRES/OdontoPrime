using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdontoPrime.Migrations
{
    /// <inheritdoc />
    public partial class LembreteEnviadoEm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LembreteEnviadoEm",
                table: "Consultas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LembreteEnviadoEm",
                table: "Consultas");
        }
    }
}
