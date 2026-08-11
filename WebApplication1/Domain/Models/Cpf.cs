using System.Linq;

namespace WebApplication1.Domain.Models;

/// <summary>
/// Utilitário de domínio para CPF: remove pontuação e valida os dígitos verificadores.
/// </summary>
public static class Cpf
{
    /// <summary>
    /// Remove tudo que não for dígito (pontos, traços, espaços).
    /// </summary>
    public static string Normalizar(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
        {
            return string.Empty;
        }

        return new string(cpf.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Valida um CPF já normalizado (somente dígitos), conferindo os dois dígitos verificadores.
    /// </summary>
    public static bool EhValido(string? cpf)
    {
        var numeros = Normalizar(cpf);

        if (numeros.Length != 11)
        {
            return false;
        }

        // CPFs com todos os dígitos iguais passam no cálculo, mas são inválidos.
        if (numeros.All(c => c == numeros[0]))
        {
            return false;
        }

        var digitos = numeros.Select(c => c - '0').ToArray();

        var primeiro = CalcularDigito(digitos, 9);
        if (primeiro != digitos[9])
        {
            return false;
        }

        var segundo = CalcularDigito(digitos, 10);
        return segundo == digitos[10];
    }

    private static int CalcularDigito(int[] digitos, int quantidade)
    {
        // Peso inicial: 10 para o 1º dígito verificador, 11 para o 2º.
        var peso = quantidade + 1;
        var soma = 0;

        for (var i = 0; i < quantidade; i++)
        {
            soma += digitos[i] * peso;
            peso--;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
