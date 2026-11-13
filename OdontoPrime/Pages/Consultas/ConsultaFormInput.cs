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
    public DateTime DataHora { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione o tipo de atendimento.")]
    public int TipoAtendimentoId { get; set; }

    [Range(15, 480, ErrorMessage = "A duração tem que ficar entre 15 e 480 minutos.")]
    public int? DuracaoMinutos { get; set; }

    [StringLength(500)]
    public string? Observacao { get; set; }
    
}
