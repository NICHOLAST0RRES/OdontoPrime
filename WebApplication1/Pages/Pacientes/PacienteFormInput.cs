using System.ComponentModel.DataAnnotations;
using WebApplication1.Validation;

namespace WebApplication1.Pages.Pacientes;

public class PacienteFormInput
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CPF.")]
    [StringLength(14, ErrorMessage = "CPF inválido.")]
    [Cpf(ErrorMessage = "CPF inválido.")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone.")]
    [StringLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione o convênio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o convênio.")]
    public int ConvenioId { get; set; }
}
