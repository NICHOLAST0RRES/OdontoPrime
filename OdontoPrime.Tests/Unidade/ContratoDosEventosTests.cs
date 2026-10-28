using System.Text.Json.Nodes;
using Clinica.Contratos;
using OdontoPrime.Infra.Mensageria.Outbox;

namespace OdontoPrime.Tests;

public class ContratoDosEventosTests
{
      private static readonly TimeSpan Salvador = TimeSpan.FromHours(-3);

    // Os mesmos valores dos arquivos em Contratos/, que são cópia dos arquivos do repositório Java.
    private static readonly Dictionary<string, object> Eventos = new()
    {
        ["consulta-agendada.json"] = new ConsultaAgendada(
            Guid.Parse("0199f000-0000-7000-8000-000000000001"),
            "Maria Teste", "71999990000", "Dr. Teste",
            new DateTimeOffset(2026, 10, 11, 10, 0, 0, Salvador),     // dataHora
            new DateTimeOffset(2026, 10, 9, 10, 0, 0, Salvador)),     // ocorridoEm

        ["consulta-cancelada.json"] = new ConsultaCancelada(
            Guid.Parse("0199f000-0000-7000-8000-000000000002"),
            "Maria Teste", "71999990000",
            new DateTimeOffset(2026, 10, 11, 10, 0, 0, Salvador),     // dataHoraOriginal
            new DateTimeOffset(2026, 10, 9, 11, 0, 0, Salvador)),     // ocorridoEm

        ["consulta-reagendada.json"] = new ConsultaReagendada(
            Guid.Parse("0199f000-0000-7000-8000-000000000003"),
            "Maria Teste", "71999990000", "Dr. Teste",
            new DateTimeOffset(2026, 10, 11, 10, 0, 0, Salvador),     // dataHoraAnterior
            new DateTimeOffset(2026, 10, 13, 9, 0, 0, Salvador),      // dataHoraNova
            new DateTimeOffset(2026, 10, 10, 18, 5, 0, Salvador)),    // ocorridoEm

        ["lembrete-de-consulta.json"] = new LembreteDeConsulta(
            Guid.Parse("0199f000-0000-7000-8000-000000000004"),
            "Maria Teste", "71999990000", "Dr. Teste",
            new DateTimeOffset(2026, 10, 11, 10, 0, 0, Salvador),     // dataHora
            new DateTimeOffset(2026, 10, 10, 18, 1, 0, Salvador)),    // ocorridoEm
    };

    [Theory]
    [InlineData("consulta-agendada.json")]
    [InlineData("consulta-cancelada.json")]
    [InlineData("consulta-reagendada.json")]
    [InlineData("lembrete-de-consulta.json")]
    public void Evento_publicado_bate_com_o_contrato(string arquivo)
    {
        // O mesmo caminho da aplicação: o JSON que entra na outbox é o JSON que o relay publica.
        var publicado = OutboxMensagem.Criar(Eventos[arquivo], "contrato", DateTime.UtcNow).Payload;
        var contrato = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Contratos", arquivo));

        Assert.True(
            JsonNode.DeepEquals(JsonNode.Parse(contrato), JsonNode.Parse(publicado)),
            $"O .NET publica um JSON diferente do contrato {arquivo}:\n{publicado}");
    }
}