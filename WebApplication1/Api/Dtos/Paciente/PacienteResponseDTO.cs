using WebApplication1.Domain.Models;

namespace WebApplication1.Api.Dtos.Paciente;

public class PacienteResponseDTO
{
    public int Id { get; set; }
    public string Nome { get;  set;}
    public string Cpf { get;  set;}
    public string Telefone { get;  set;}
    public string Email { get;  set; }
    public string ConvenioNome { get;  set;}
    public int ConvenioId { get;  set;}
}