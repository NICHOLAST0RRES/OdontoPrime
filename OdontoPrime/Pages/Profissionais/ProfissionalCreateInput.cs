using System.ComponentModel.DataAnnotations;

namespace OdontoPrime.Pages.Profissionais;

public class ProfissionalCreateInput
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o telefone.")]
    [StringLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione o tipo de profissional.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o tipo de profissional.")]
    public int TipoProfissionalId { get; set; }

    [StringLength(20)]
    public string? Cro { get; set; }
}
