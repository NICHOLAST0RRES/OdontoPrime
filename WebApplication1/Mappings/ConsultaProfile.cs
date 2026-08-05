using AutoMapper;
using WebApplication1.Api.Dtos.Consulta;
using WebApplication1.Domain.Models;

namespace WebApplication1.Mappings;

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