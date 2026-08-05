using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Pages.Profissionais;

public class ProfissionalEditInput
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone.")]
    [StringLength(20)]
    public string Telefone { get; set; } = string.Empty;

    public string TipoProfissional { get; set; } = string.Empty;

    public string? Cro { get; set; }
}
