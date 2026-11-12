using OdontoPrime.Domain.Models;

namespace OdontoPrime.Tests;

public class ConsultaDuracaoTests
{
    private static readonly DateTime Agora = new(2026, 10, 9, 13, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Inicio = new(2026, 10, 11, 13, 0, 0, DateTimeKind.Utc);

    private static readonly TipoAtendimento Limpeza = new()
    {
        Id = TipoAtendimento.LimpezaId, Nome = "Limpeza", DuracaoPadraoMinutos = 45, DuracaoMaximaMinutos = 90
    };

    private static Consulta NovaConsulta(int? duracaoMinutos)
        => new(Guid.NewGuid(), Guid.NewGuid(), Limpeza, duracaoMinutos, Inicio, observacao: null, Agora, lembrarEm: null);

    [Fact]
    public void Sem_duracao_informada_usa_a_padrao_do_tipo()                    // 1.
    {
        Assert.Equal(Inicio.AddMinutes(45), NovaConsulta(duracaoMinutos: null).Fim);
    }

    [Fact]
    public void Aceita_duracao_ajustada_ate_o_maximo()                          // 2.
    {
        Assert.Equal(Inicio.AddMinutes(90), NovaConsulta(duracaoMinutos: 90).Fim);
    }

    [Theory]                                                                    // 3.
    [InlineData(105)]   // acima do máximo da limpeza
    [InlineData(50)]    // não é múltiplo de 15
    [InlineData(0)]
    [InlineData(-15)]
    public void Recusa_duracao_invalida(int minutos)
    {
        Assert.Throws<ArgumentException>(() => NovaConsulta(minutos));
    }

    [Fact]
    public void Reagendar_mantem_a_duracao()                                    // 4.
    {
        var consulta = NovaConsulta(duracaoMinutos: 60);
        var novoInicio = Inicio.AddDays(1);

        consulta.Reagendar(novoInicio, Agora, lembrarEm: null);

        Assert.Equal(novoInicio.AddMinutes(60), consulta.Fim);
    }
}