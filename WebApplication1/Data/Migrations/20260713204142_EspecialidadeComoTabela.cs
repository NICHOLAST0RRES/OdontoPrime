using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApplication1.Data.Migrations
{
    /// <inheritdoc />
    public partial class EspecialidadeComoTabela : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Especialidade",
                table: "Medicos");

            migrationBuilder.AddColumn<int>(
                name: "EspecialidadeId",
                table: "Medicos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Especialidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especialidades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_EspecialidadeId",
                table: "Medicos",
                column: "EspecialidadeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medicos_Especialidades_EspecialidadeId",
                table: "Medicos",
                column: "EspecialidadeId",
                principalTable: "Especialidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medicos_Especialidades_EspecialidadeId",
                table: "Medicos");

            migrationBuilder.DropTable(
                name: "Especialidades");

            migrationBuilder.DropIndex(
                name: "IX_Medicos_EspecialidadeId",
                table: "Medicos");

            migrationBuilder.DropColumn(
                name: "EspecialidadeId",
                table: "Medicos");

            migrationBuilder.AddColumn<int>(
                name: "Especialidade",
                table: "Medicos",
                type: "integer",
                maxLength: 128,
                nullable: false,
                defaultValue: 0);
        }
    }
}
