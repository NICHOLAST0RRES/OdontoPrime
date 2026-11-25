using Microsoft.Extensions.DependencyInjection;
using OdontoPrime.Application;
using OdontoPrime.Data;
using OdontoPrime.Domain.Models;


namespace OdontoPrime.Tests;

[Collection(ColecaoDeIntegracao.Nome)]
public class BloqueioServiceTests(InfraDeTeste infra) : TesteDeIntegracao(infra)
{
    // Bloqueio padrão: 20/10, das 8h às 18h na clínica. O relógio dos testes está em 09/10.
    private static readonly DateTime InicioUtc = NaClinica(10, 20, 8).UtcDateTime;
    private static readonly DateTime FimUtc = NaClinica(10, 20, 18).UtcDateTime;

    [Fact]
    public async Task Cria_bloqueio_para_dentista()
    {
        var (_, dentistaId) = await CriarPacienteEDentistaAsync();

        var resultado = await BloqueiosAsync(s => s.CriarAsync(dentistaId, InicioUtc, FimUtc, "Congresso"));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Single(await BloqueiosAsync(s => s.ListarFuturosAsync(dentistaId)));
    }

    [Fact]
    public async Task Recusa_bloqueio_sobre_consulta_agendada()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 20, 10));

        var resultado = await BloqueiosAsync(s => s.CriarAsync(dentistaId, InicioUtc, FimUtc, "Férias"));

        Assert.Equal(TipoError.Conflito, resultado.TipoErro);
    }

    [Fact]
    public async Task Aceita_bloqueio_sobre_consulta_cancelada()
    {
        var (pacienteId, dentistaId) = await CriarPacienteEDentistaAsync();
        var consultaId = await AgendarAsync(pacienteId, dentistaId, NaClinica(10, 20, 10));
        await ConsultasAsync(s => s.CancelarAsync(consultaId));

        var resultado = await BloqueiosAsync(s => s.CriarAsync(dentistaId, InicioUtc, FimUtc, "Férias"));

        Assert.True(resultado.Sucesso, resultado.Erro);
    }

    [Fact]
    public async Task Recusa_bloqueio_para_atendente()
    {
        var atendenteId = await CriarAtendenteAsync();

        var resultado = await BloqueiosAsync(s => s.CriarAsync(atendenteId, InicioUtc, FimUtc, "Férias"));

        Assert.Equal(TipoError.Invalido, resultado.TipoErro);
    }

    [Fact]
    public async Task Remover_apaga_o_bloqueio()
    {
        var (_, dentistaId) = await CriarPacienteEDentistaAsync();
        var criado = await BloqueiosAsync(s => s.CriarAsync(dentistaId, InicioUtc, FimUtc, "Congresso"));

        var resultado = await BloqueiosAsync(s => s.RemoverAsync(criado.Valor!.Id));

        Assert.True(resultado.Sucesso, resultado.Erro);
        Assert.Empty(await BloqueiosAsync(s => s.ListarFuturosAsync(dentistaId)));
    }

    // ---------- Apoio ----------

    private async Task<T> BloqueiosAsync<T>(Func<BloqueioService, Task<T>> acao)
    {
        using var escopo = NovoEscopo();
        return await acao(escopo.ServiceProvider.GetRequiredService<BloqueioService>());
    }

    private async Task<Result> ConsultasAsync(Func<ConsultaService, Task<Result>> acao)
    {
        using var escopo = NovoEscopo();
        return await acao(escopo.ServiceProvider.GetRequiredService<ConsultaService>());
    }

    private async Task<Guid> CriarAtendenteAsync()
    {
        using var escopo = NovoEscopo();
        var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();

        var atendente = new Profissional("João", "71900000001", TipoProfissional.AtendenteId, cro: null);
        db.Profissionais.Add(atendente);
        await db.SaveChangesAsync(Ct);

        return atendente.Id;
    }
}