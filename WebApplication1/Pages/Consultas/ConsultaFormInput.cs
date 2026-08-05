using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Pages.Consultas;

public class ConsultaFormInput
{
    [Required(ErrorMessage = "Selecione o paciente.")]
    public Guid PacienteId { get; set; }

    [Required(ErrorMessage = "Selecione o profissional.")]
    public Guid ProfissionalId { get; set; }

    [Required(ErrorMessage = "Informe a data e hora.")]
    [DataType(DataType.DateTime)]
    public DateTime DataHora { get; set; }

    [StringLength(500)]
    public string? Observacao { get; set; }
}
