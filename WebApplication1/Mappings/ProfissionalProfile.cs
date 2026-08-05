using AutoMapper;
using WebApplication1.Api.Dtos.TipoProfissional;
using WebApplication1.Domain.Models;

namespace WebApplication1.Mappings;

public class ProfissionalProfile :  Profile
{
    public ProfissionalProfile()
    {
        CreateMap<Profissional, ProfissionalResponseDTO>()
            .ForMember(
                dest => dest.TipoProfissional,
                opt => opt.MapFrom(src => src.TipoProfissional.Nome)
            );
    }
}