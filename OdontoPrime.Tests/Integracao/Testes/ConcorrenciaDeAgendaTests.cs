using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OdontoPrime.Application;
using OdontoPrime.Data;
using Xunit;

namespace OdontoPrime.Tests;


    // Corrida: duas marcações simultâneas pro mesmo dentista e horário.
// Só o banco consegue garantir que uma delas perde.
    [Collection(ColecaoDeIntegracao.Nome)]
    public class ConcorrenciaDeAgendaTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
    {

// 1. Que exatamente uma das marcações teve sucesso
// 2. Que todas as outras foram recusadas como conflito de horário
// 3. Que o banco ficou com uma consulta só pra esse dentista
        [Fact]
        public async Task Marcacoes_simultaneas_no_mesmo_horario_gravam_uma_so()
        {
            // Preparar
            const int recepcionistas = 10;
            var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
            var horario = NaClinica(10, 11, 10).UtcDateTime;   // domingo, 11/10, 10h

            // Agir: as 10 começam juntas; o portão só libera as gravações quando todas tiverem checado
            var portao = Infra.App.Services.GetRequiredService<PortaoDeGravacao>();
            portao.Armar(recepcionistas);

            Result[] resultados;
            try
            {
                resultados = await Task.WhenAll(
                    Enumerable.Range(0, recepcionistas)
                        .Select(_ => AgendarEmEscopoProprioAsync(pacienteId, dentistaId, horario)));
            }
            finally
            {
                portao.Desarmar();   // o próximo teste não pode encontrar o portão armado
            }

            // Verificar
            Assert.Single(resultados, r => r.Sucesso);                                              // 1.
            Assert.Equal(recepcionistas - 1, resultados.Count(r => r.TipoErro == TipoError.Conflito)); // 2.
            Assert.Equal(1, await ConsultasDoDentistaAsync(dentistaId));                            // 3.
        }

        private async Task<Result> AgendarEmEscopoProprioAsync(Guid pacienteId, Guid dentistaId, DateTime horarioUtc)
        {
            using var escopo = NovoEscopo();
            var servico = escopo.ServiceProvider.GetRequiredService<ConsultaService>();
            return await servico.AgendarAsync(pacienteId, dentistaId, horarioUtc, observacao: null);
        }

        private async Task<int> ConsultasDoDentistaAsync(Guid dentistaId)
        {
            using var escopo = NovoEscopo();
            var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
            return await db.Consultas.CountAsync(c => c.ProfissionalId == dentistaId, Ct);
        }
    }
