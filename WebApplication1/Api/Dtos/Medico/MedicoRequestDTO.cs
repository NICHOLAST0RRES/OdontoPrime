using WebApplication1.Domain.Models;

namespace WebApplication1.Api.Dtos.Medico;

public class MedicoRequestDTO
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Crm { get; set; }
    public DateOnly DataNascimento { get; set; }
    public int EspecialidadeId { get; set; }

}