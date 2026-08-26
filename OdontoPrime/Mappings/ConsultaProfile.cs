using AutoMapper;
using OdontoPrime.Api.Dtos.Consulta;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Mappings;

public class ConsultaProfile  : Profile
{
    public ConsultaProfile()
    {
        CreateMap<Consulta, ConsultaResponseDTO>()
            .ForMember(d => d.PacienteNome, o => o.MapFrom(s => s.Paciente.Nome))
            .ForMember(d => d.ProfissionalNome, o => o.MapFrom(s => s.Profissional.Nome))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.StatusConsulta.Nome));
    }
}