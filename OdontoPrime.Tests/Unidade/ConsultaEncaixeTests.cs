using OdontoPrime.Domain.Models;

namespace OdontoPrime.Tests;

// Função: testar as regras do encaixe na própria Consulta.
// Protege: encaixe sem motivo (sem rastro de quem decidiu e por quê) e consulta que continua
// marcada como encaixe depois de ser reagendada pra um horário normal.
public class ConsultaEncaixeTests
{
    private static readonly DateTime Agora = new(2026, 10, 9, 13, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Inicio = new(2026, 10, 12, 22, 0, 0, DateTimeKind.Utc);   // 19h na clínica

    private static readonly TipoAtendimento Avaliacao = new()
    {
        Id = TipoAtendimento.AvaliacaoId, Nome = "Avaliação", DuracaoPadraoMinutos = 30, DuracaoMaximaMinutos = 60
    };

    private static Consulta NovaConsulta()
        => new(Guid.NewGuid(), Guid.NewGuid(), Avaliacao, duracaoMinutos: null, Inicio, observacao: null, Agora, lembrarEm: null);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Encaixe_exige_motivo(string motivo)
    {
        Assert.Throws<ArgumentException>(() => NovaConsulta().MarcarComoEncaixe(motivo));
    }

    [Fact]
    public void Reagendar_limpa_o_encaixe()
    {
        var consulta = NovaConsulta();
        consulta.MarcarComoEncaixe("Dor forte, paciente não pode esperar");

        consulta.Reagendar(Inicio.AddDays(1), Agora, lembrarEm: null);

        Assert.False(consulta.Encaixe);
        Assert.Null(consulta.MotivoEncaixe);
    }
}
