using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OdontoPrime.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaTipoAtendimentoEFim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Fim",
                table: "Consultas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            
            migrationBuilder.Sql("""UPDATE "Consultas" SET "Fim" = "DataHora" + interval '30 minutes';""");

            migrationBuilder.AddColumn<int>(
                name: "TipoAtendimentoId",
                table: "Consultas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "TiposAtendimento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DuracaoPadraoMinutos = table.Column<int>(type: "integer", nullable: false),
                    DuracaoMaximaMinutos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposAtendimento", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TiposAtendimento",
                columns: new[] { "Id", "DuracaoMaximaMinutos", "DuracaoPadraoMinutos", "Nome" },
                values: new object[,]
                {
                    { 1, 60, 30, "Avaliação" },
                    { 2, 90, 45, "Limpeza" },
                    { 3, 120, 60, "Restauração" },
                    { 4, 180, 90, "Canal" },
                    { 5, 60, 30, "Urgência" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_TipoAtendimentoId",
                table: "Consultas",
                column: "TipoAtendimentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultas_TiposAtendimento_TipoAtendimentoId",
                table: "Consultas",
                column: "TipoAtendimentoId",
                principalTable: "TiposAtendimento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            
            // Os defaults só serviam pra preencher as consultas que já existiam.
            // Daqui pra frente, quem inserir sem informar essas colunas recebe erro.
            migrationBuilder.Sql("""
                                 ALTER TABLE "Consultas" ALTER COLUMN "Fim" DROP DEFAULT;
                                 ALTER TABLE "Consultas" ALTER COLUMN "TipoAtendimentoId" DROP DEFAULT;
                                 """);
        }
        

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultas_TiposAtendimento_TipoAtendimentoId",
                table: "Consultas");

            migrationBuilder.DropTable(
                name: "TiposAtendimento");

            migrationBuilder.DropIndex(
                name: "IX_Consultas_TipoAtendimentoId",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "Fim",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "TipoAtendimentoId",
                table: "Consultas");
        }
    }
}
