using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AutoMapper;

namespace AROCONSTRUCCIONES.Services.Mapping_Profile
{
    public class RequerimientoProfile : Profile
    {
        public RequerimientoProfile()
        {
            // Mapeo para el Detalle
            CreateMap<DetalleRequerimientoDto, DetalleRequerimiento>().ReverseMap();

            // Mapeo para el Maestro (el formulario)
            CreateMap<RequerimientoCreateDto, Requerimiento>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));

            CreateMap<Requerimiento, RequerimientoCreateDto>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));
            CreateMap<Requerimiento, RequerimientoListDto>();
        }
    }
}
