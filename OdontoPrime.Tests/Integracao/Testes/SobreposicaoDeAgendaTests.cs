using Microsoft.Extensions.DependencyInjection;
using OdontoPrime.Application;
using OdontoPrime.Data;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Tests;

// Regras de sobreposição da agenda. Todas as consultas aqui são avaliações: 30 minutos.
[Collection(ColecaoDeIntegracao.Nome)]
public class SobreposicaoDeAgendaTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
    // O que ele verifica?
    // 1. Que uma consulta começando no meio de outra é recusada como conflito
    [Fact]
    public async Task Sobreposicao_parcial_e_recusada()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10));          // 10h00–10h30

        var resultado = await TentarAgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10, 15));

        Assert.Equal(TipoError.Conflito, resultado.TipoErro);                       // 1.
    }

    // O que ele verifica?
    // 1. Que uma consulta começando exatamente quando a outra termina é aceita: o intervalo é [início, fim)
    [Fact]
    public async Task Consultas_encostadas_sao_aceitas()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10));          // 10h00–10h30

        var resultado = await TentarAgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10, 30));

        Assert.True(resultado.Sucesso, resultado.Erro);                             // 1.
    }

    // O que ele verifica?
    // 1. Que outro dentista pode atender no mesmo horário
    [Fact]
    public async Task Outro_dentista_no_mesmo_horario_e_aceito()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        var outroDentistaId = await CriarOutroDentistaAsync();
        await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10));

        var resultado = await TentarAgendarAsync(pacienteId, outroDentistaId, NaClinica(10, 11, 10));

        Assert.True(resultado.Sucesso, resultado.Erro);                             // 1.
    }

    // O que ele verifica?
    // 1. Que cancelar uma consulta libera o horário pra outra
    [Fact]
    public async Task Cancelar_libera_o_horario()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        var consultaId = await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10));
        await ServicoAsync(s => s.CancelarAsync(consultaId));

        var resultado = await TentarAgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10));

        Assert.True(resultado.Sucesso, resultado.Erro);                             // 1.
    }

    // O que ele verifica?
    // 1. Que reagendar uma consulta pra cima de outra é recusado como conflito
    [Fact]
    public async Task Reagendar_para_cima_de_outra_e_recusado()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 10));          // 10h00–10h30
        var segundaId = await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 11, 11));

        var resultado = await ServicoAsync(s => s.ReagendarAsync(segundaId, NaClinica(10, 11, 10, 15).UtcDateTime));

        Assert.Equal(TipoError.Conflito, resultado.TipoErro);                       // 1.
    }

    // ---------- Apoio ----------

    // Diferente do AgendarAsync da base, devolve o resultado em vez de exigir sucesso.
    private Task<Result> TentarAgendarAsync(Guid pacienteId, Guid dentistaId, DateTimeOffset dataHora)
        => ServicoAsync(async s => (Result)await s.AgendarAsync(pacienteId, dentistaId, dataHora.UtcDateTime,
            TipoAtendimento.AvaliacaoId, duracaoMinutos: null, observacao: null));

    private async Task<Result> ServicoAsync(Func<ConsultaService, Task<Result>> acao)
    {
        using var escopo = NovoEscopo();
        return await acao(escopo.ServiceProvider.GetRequiredService<ConsultaService>());
    }

    private async Task<Guid> CriarOutroDentistaAsync()
    {
        using var escopo = NovoEscopo();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();

        var dentista = new Profissional("Dra. Outra", "71977770000", TipoProfissional.DentistaId, "CRO-BA 5678");
        db.Profissionais.Add(dentista);
        await db.SaveChangesAsync(Ct);

        return dentista.Id;
    }
}