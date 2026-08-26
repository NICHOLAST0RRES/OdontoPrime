using AutoMapper;
using OdontoPrime.Api.Dtos.Paciente;
using OdontoPrime.Domain.Models;

namespace OdontoPrime.Mappings;

public class PacienteProfile :  Profile
{
    public PacienteProfile()
    {
        CreateMap<Paciente, PacienteResponseDTO>()
            
            // faco isso para a requisicao exibir o nome cardiologia enves do id
            .ForMember(dest => dest.ConvenioNome, opt => opt.MapFrom(src => src.Convenio.Nome));
        
        // for write
        CreateMap<PacienteRequestDTO, Paciente>(); 
    }
    
}