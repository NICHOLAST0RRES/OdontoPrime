using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Pages.Pacientes;

public class PacienteFormInput
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o CPF.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter 11 dígitos, sem pontuação.")]
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
