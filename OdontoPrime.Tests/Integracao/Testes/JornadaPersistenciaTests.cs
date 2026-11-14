using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OdontoPrime.Data;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Tests;
[Collection(ColecaoDeIntegracao.Nome)]
public class JornadaPersistenciaTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
        [Fact]
        public async Task Jornada_e_gravada_e_carregada_junto_com_o_dentista()
        {
            // Gravar
            Guid dentistaId;
            using (var escopo = NovoEscopo())
            {
                var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
                var dentista = new Profissional("Dra. Ana", "71900000000", TipoProfissional.DentistaId, "CRO-BA 1111");
                dentista.DefinirJornada([
                    new PeriodoDeTrabalho(DayOfWeek.Monday, new TimeOnly(8, 0), new TimeOnly(12, 0)),
                    new PeriodoDeTrabalho(DayOfWeek.Monday, new TimeOnly(14, 0), new TimeOnly(18, 0))
                ]);
                db.Profissionais.Add(dentista);
                await db.SaveChangesAsync(Ct);
                dentistaId = dentista.Id;
            }

            // Ler de novo, em outro escopo
            using (var escopo = NovoEscopo())
            {
                var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
                var dentista = await db.Profissionais.SingleAsync(p => p.Id == dentistaId, Ct);

                Assert.Equal(2, dentista.Jornada.Count);
                Assert.Contains(dentista.Jornada, p => p.Dia == DayOfWeek.Monday && p.Inicio == new TimeOnly(14, 0));
            }
        }
}