using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OdontoPrime.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaJornadaDosProfissionais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JornadaDosProfissionais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Dia = table.Column<int>(type: "integer", nullable: false),
                    Inicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Fim = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ProfissionalId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JornadaDosProfissionais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JornadaDosProfissionais_Profissionais_ProfissionalId",
                        column: x => x.ProfissionalId,
                        principalTable: "Profissionais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JornadaDosProfissionais_ProfissionalId",
                table: "JornadaDosProfissionais",
                column: "ProfissionalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JornadaDosProfissionais");
        }
    }
}
