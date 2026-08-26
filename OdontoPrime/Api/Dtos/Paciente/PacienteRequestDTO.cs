using OdontoPrime.Domain.Models;

namespace OdontoPrime.Api.Dtos.Paciente;

public record PacienteRequestDTO
{
    public Guid Id { get; set; }
    public string Nome { get;  set;}
    public string Cpf { get;  set;}
    public string Telefone { get;  set;}
    public string Email { get;  set; }
    public int ConvenioId { get;  set;}
}