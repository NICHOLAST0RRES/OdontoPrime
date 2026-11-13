using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OdontoPrime.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaRestricaoSemSobreposicao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // O EF não modela exclusion constraint: ela entra como SQL.
            // "Mesmo dentista (=) e intervalos que se sobrepõem (&&)" não podem coexistir,
            // contando só as consultas agendadas e ativas: cancelar libera o horário.
            migrationBuilder.Sql("""
                                 CREATE EXTENSION IF NOT EXISTS btree_gist;

                                 ALTER TABLE "Consultas"
                                     ADD CONSTRAINT "EX_Consultas_SemSobreposicao"
                                     EXCLUDE USING gist (
                                         "ProfissionalId" WITH =,
                                         tstzrange("DataHora", "Fim", '[)') WITH &&
                                     )
                                     WHERE ("StatusConsultaId" = 1 AND "Ativo");
                                 """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE "Consultas" DROP CONSTRAINT "EX_Consultas_SemSobreposicao";""");
        }
    }
}
