using AutoMapper;
using WebApplication1.Api.Dtos.Medico;
using WebApplication1.Domain.Models;

namespace WebApplication1.Mappings;

//automapper
public class MedicoProfile : Profile
{
    public  MedicoProfile()
    {
        // for read 
        CreateMap<Medico, MedicoResponseDTO>()
            
            // faco isso para a requisicao exibir o nome cardiologia enves do id
            .ForMember(dest => dest.EspecialidadeNome, opt => opt.MapFrom(src => src.Especialidade.Nome));
        
        // for write
        CreateMap<MedicoRequestDTO, Medico>(); 

        
    }
}