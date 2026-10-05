using System.Text.Json;

namespace OdontoPrime.Infra.Mensageria;

// transforma a propriedade exemplo : PacienteNome do C# em pacienteNome, record que o Java espera.
public static class OpcoesJsonMensageria
{
    public static readonly JsonSerializerOptions Padrao = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}