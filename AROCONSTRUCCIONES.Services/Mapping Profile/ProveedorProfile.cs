using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AutoMapper;

namespace AROCONSTRUCCIONES.Services.Mapping_Profile
{
    public class ProveedorProfile : Profile
    {
        public ProveedorProfile()
        {
            CreateMap<Proveedor, ProveedorDto>().ReverseMap();
        }
    }
}
