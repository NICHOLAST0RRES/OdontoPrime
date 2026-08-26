using OdontoPrime.Domain.Models;

namespace OdontoPrime.Api.Dtos.Medico;

public record MedicoRequestDTO 
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Crm { get; set; }
    public DateOnly DataNascimento { get; set; }
    public int EspecialidadeId { get; set; }

}