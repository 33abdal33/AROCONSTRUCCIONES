using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Mapping_Profile
{
    public class OrdenCompraProfile : Profile
    {
        public OrdenCompraProfile()
        {
            // 1. Mapeo para el Listado (Entidad -> DTO)
            CreateMap<OrdenCompra, OrdenCompraListDto>()
                .ForMember(dest => dest.ProveedorRazonSocial,
                           opt => opt.MapFrom(src => src.Proveedor != null ? src.Proveedor.RazonSocial : "N/A"))
                .ForMember(dest => dest.RutaPdf, opt => opt.MapFrom(src => src.RutaPdf)); // <-- ¡AÑADE ESTA LÍNEA!

            // 2. Mapeo para los Detalles (se usa en el de Creación)
            CreateMap<DetalleOrdenCompraDto, DetalleOrdenCompra>();
            CreateMap<DetalleOrdenCompra, DetalleOrdenCompraDto>(); // Para futuras vistas de "Detalle"

            // 3. Mapeo para la Creación (DTO -> Entidad)
            // Este es el mapeo clave que construye el objeto anidado
            CreateMap<OrdenCompraCreateDto, OrdenCompra>()
                .ForMember(dest => dest.Detalles,
                           opt => opt.MapFrom(src => src.Detalles)); // Mapea la lista de DTOs a la lista de Entidades
                                                                     // 1. Mapeo para el detalle del modal
            CreateMap<DetalleOrdenCompra, RecepcionDetalleDto>()
                .ForMember(dest => dest.DetalleOrdenCompraId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.MaterialId, opt => opt.MapFrom(src => src.IdMaterial))
                .ForMember(dest => dest.MaterialNombre,
                            opt => opt.MapFrom(src => src.Material != null ? src.Material.Nombre : "N/A"))
                .ForMember(dest => dest.UnidadMedida,
                            opt => opt.MapFrom(src => src.Material != null ? src.Material.UnidadMedida : "N/A"))
                .ForMember(dest => dest.CantidadPedida, opt => opt.MapFrom(src => src.Cantidad))
                .ForMember(dest => dest.CantidadYaRecibida, opt => opt.MapFrom(src => src.CantidadRecibida))
                .ForMember(dest => dest.CantidadARecibir, opt => opt.Ignore()); // El usuario lo llena

            // 2. Mapeo para el maestro del modal
            CreateMap<OrdenCompra, RecepcionMaestroDto>()
                .ForMember(dest => dest.OrdenCompraId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CodigoOC, opt => opt.MapFrom(src => src.Codigo))
                .ForMember(dest => dest.ProveedorNombre,
                            opt => opt.MapFrom(src => src.Proveedor != null ? src.Proveedor.RazonSocial : "N/A"))
                .ForMember(dest => dest.ProveedorId, opt => opt.MapFrom(src => src.IdProveedor))
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));
        }
    }
}
