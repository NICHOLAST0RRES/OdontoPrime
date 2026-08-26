using AutoMapper;
using OdontoPrime.Api.Dtos.TipoProfissional;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Mappings;

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