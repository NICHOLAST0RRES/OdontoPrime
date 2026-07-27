using AutoMapper;
using WebApplication1.Api.Dtos.Paciente;
using WebApplication1.Domain.Models;

namespace WebApplication1.Mappings;

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