using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColunasAuditoriaPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DesativadoEm",
                table: "Medicos",
                newName: "CriadoEm");

            migrationBuilder.AddColumn<DateTime>(
                name: "AtualizadoEm",
                table: "Pacientes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CriadoEm",
                table: "Pacientes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "AtualizadoEm",
                table: "Medicos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletadoEm",
                table: "Medicos",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AtualizadoEm",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "CriadoEm",
                table: "Pacientes");

            migrationBuilder.DropColumn(
                name: "AtualizadoEm",
                table: "Medicos");

            migrationBuilder.DropColumn(
                name: "DeletadoEm",
                table: "Medicos");

            migrationBuilder.RenameColumn(
                name: "CriadoEm",
                table: "Medicos",
                newName: "DesativadoEm");
        }
    }
}
