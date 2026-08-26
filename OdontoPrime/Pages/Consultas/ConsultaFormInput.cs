using System.ComponentModel.DataAnnotations;

namespace OdontoPrime.Pages.Consultas;

public class ConsultaFormInput
{
    [Required(ErrorMessage = "Selecione o paciente.")]
    public Guid PacienteId { get; set; }

    [Required(ErrorMessage = "Selecione o profissional.")]
    public Guid ProfissionalId { get; set; }

    [Required(ErrorMessage = "Informe a data e hora.")]
    [DataType(DataType.DateTime)]
    public DateTime DataHora { get; set;} = DateTime.Now.AddSeconds(-DateTime.Now.Second)
        .AddMilliseconds(-DateTime.Now.Millisecond);
    
    [StringLength(500)]
    public string? Observacao { get; set; }
}
