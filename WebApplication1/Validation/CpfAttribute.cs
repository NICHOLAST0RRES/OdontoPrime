using System.ComponentModel.DataAnnotations;
using DomainCpf = WebApplication1.Domain.Models.Cpf;

namespace WebApplication1.Validation;

/// <summary>
/// Valida um CPF aceitando com ou sem pontuação, conferindo os dígitos verificadores.
/// </summary>
public class CpfAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // A obrigatoriedade fica por conta do [Required]; aqui só validamos quando há valor.
        if (value is not string cpf || string.IsNullOrWhiteSpace(cpf))
        {
            return ValidationResult.Success;
        }

        if (!DomainCpf.EhValido(cpf))
        {
            return new ValidationResult(
                ErrorMessage ?? "CPF inválido.",
                new[] { validationContext.MemberName! });
        }

        return ValidationResult.Success;
    }
}
