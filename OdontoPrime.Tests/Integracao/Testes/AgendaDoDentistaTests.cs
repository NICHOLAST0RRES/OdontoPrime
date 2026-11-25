using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OdontoPrime.Application;
using OdontoPrime.Data;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Tests;

// Função: testar o ConsultaService com o Postgres real, numa dentista com jornada comercial
// (seg–sex, 08–12 e 14–18): recusa fora da jornada, encaixe, bloqueio e reagendamento.
// Protege: consulta fora da jornada sem confirmação, encaixe gravado sem motivo, encaixe passando
// por cima de bloqueio, reagendamento contornando a jornada e motivo virando encaixe sem necessidade.
// Datas: o relógio dos testes está na sexta, 09/10. A segunda é 12/10 e o sábado é 17/10.
[Collection(ColecaoDeIntegracao.Nome)]
public class AgendaDoDentistaTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
    [Fact]
    public async Task Fora_da_jornada_sem_encaixe_e_recusado()
    {
        var (pacienteId, dentistaId) = await CriarCenarioAsync();

        var resultado = await TentarAgendarAsync(pacienteId, dentistaId, NaClinica(10, 17, 10));   // sábado

        Assert.Equal(TipoError.ForaDaJornada, resultado.TipoErro);
    }

    [Fact]
    public async Task Fora_da_jornada_com_motivo_vira_encaixe()
    {
        var (pacienteId, dentistaId) = await CriarCenarioAsync();

        var resultado = await TentarAgendarAsync(pacienteId, dentistaId, NaClinica(10, 12, 19), "Dor forte");   // segunda, 19h

        Assert.True(resultado.Sucesso, resultado.Erro);
        var consulta = await ConsultaGravadaAsync(resultado.Valor!.Id);
        Assert.True(consulta.Encaixe);
        Assert.Equal("Dor forte", consulta.MotivoEncaixe);
    }

    [Fact]
    public async Task Bloqueio_recusa_mesmo_com_encaixe()
    {
        var (pacienteId, dentistaId) = await CriarCenarioAsync();
        await BloquearAsync(dentistaId, NaClinica(10, 12, 8), NaClinica(10, 12, 18));

        var resultado = await TentarAgendarAsync(pacienteId, dentistaId, NaClinica(10, 12, 10), "Paciente insistiu");

        Assert.Equal(TipoError.Conflito, resultado.TipoErro);
    }

    [Fact]
    public async Task Reagendar_para_fora_da_jornada_sem_encaixe_e_recusado()
    {
        var (pacienteId, dentistaId) = await CriarCenarioAsync();
        var agendada = await TentarAgendarAsync(pacienteId, dentistaId, NaClinica(10, 12, 10));
        Assert.True(agendada.Sucesso, agendada.Erro);

        var resultado = await ServicoAsync(s => s.ReagendarAsync(agendada.Valor!.Id, NaClinica(10, 12, 13).UtcDateTime));   // almoço

        Assert.Equal(TipoError.ForaDaJornada, resultado.TipoErro);
    }

    [Fact]
    public async Task Motivo_dentro_da_jornada_nao_marca_encaixe()
    {
        var (pacienteId, dentistaId) = await CriarCenarioAsync();

        var resultado = await TentarAgendarAsync(pacienteId, dentistaId, NaClinica(10, 12, 10), "Não precisava");

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.False((await ConsultaGravadaAsync(resultado.Valor!.Id)).Encaixe);
    }

    // ---------- Apoio ----------

    // Paciente do helper padrão e uma dentista com jornada comercial: seg–sex, 08–12 e 14–18.
    private async Task<(Guid PacienteId, Guid DentistaId)> CriarCenarioAsync()
    {
        var (pacienteId, _) = await CriarPacienteEDentistaAsync();

        using var escopo = NovoEscopo();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();

        var dentista = new Profissional("Dra. Ana", "71900000000", TipoProfissional.DentistaId, "CRO-BA 1111");
        dentista.DefinirJornada(
            from dia in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }
            from periodo in new[] { (Inicio: 8, Fim: 12), (Inicio: 14, Fim: 18) }
            select new PeriodoDeTrabalho(dia, new TimeOnly(periodo.Inicio, 0), new TimeOnly(periodo.Fim, 0)));

        db.Profissionais.Add(dentista);
        await db.SaveChangesAsync(Ct);

        return (pacienteId, dentista.Id);
    }

    private Task<Result<Consulta>> TentarAgendarAsync(Guid pacienteId, Guid dentistaId, DateTimeOffset dataHora, string? motivoEncaixe = null)
        => ServicoAsync(s => s.AgendarAsync(pacienteId, dentistaId, dataHora.UtcDateTime,
            TipoAtendimento.AvaliacaoId, duracaoMinutos: null, observacao: null, motivoEncaixe));

    private async Task<T> ServicoAsync<T>(Func<ConsultaService, Task<T>> acao)
    {
        using var escopo = NovoEscopo();
        return await acao(escopo.ServiceProvider.GetRequiredService<ConsultaService>());
    }

    private async Task BloquearAsync(Guid dentistaId, DateTimeOffset inicio, DateTimeOffset fim)
    {
        using var escopo = NovoEscopo();
        var resultado = await escopo.ServiceProvider.GetRequiredService<BloqueioService>()
            .CriarAsync(dentistaId, inicio.UtcDateTime, fim.UtcDateTime, "Congresso");
        Assert.True(resultado.Sucesso, resultado.Erro);
    }

    // Lê de um escopo novo: prova que o encaixe foi gravado no banco, não só marcado em memória.
    private async Task<Consulta> ConsultaGravadaAsync(Guid id)
    {
        using var escopo = NovoEscopo();
        return await escopo.ServiceProvider.GetRequiredService<AppDbContext>()
            .Consultas.AsNoTracking().SingleAsync(c => c.Id == id, Ct);
    }
}
