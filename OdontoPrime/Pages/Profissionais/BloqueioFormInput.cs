using System.ComponentModel.DataAnnotations;

namespace OdontoPrime.Pages.Profissionais;

// Formulário de novo bloqueio, no horário da clínica.
public class BloqueioFormInput
{
    [Required(ErrorMessage = "Informe o início.")]
    [Display(Name = "Início")]
    public DateTime? Inicio { get; set; }

    [Required(ErrorMessage = "Informe o fim.")]
    [Display(Name = "Fim")]
    public DateTime? Fim { get; set; }

    [Required(ErrorMessage = "Informe o motivo.")]
    [StringLength(200, ErrorMessage = "O motivo pode ter no máximo 200 caracteres.")]
    [Display(Name = "Motivo")]
    public string Motivo { get; set; } = "";
}
