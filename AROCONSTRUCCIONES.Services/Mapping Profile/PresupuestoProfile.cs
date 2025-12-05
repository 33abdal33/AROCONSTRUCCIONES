using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AutoMapper;

namespace AROCONSTRUCCIONES.Services.Mapping_Profile
{
    public class PresupuestoProfile : Profile
    {
        public PresupuestoProfile()
        {
            CreateMap<Partida, PartidaDto>().ReverseMap();
        }
    }
}