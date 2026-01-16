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
                .ForMember(dest => dest.RutaPdf, opt => opt.MapFrom(src => src.RutaPdf));

            // --- 2. MAPEO CRÍTICO QUE FALTABA (DTO CREACIÓN -> ENTIDAD) ---
            // AutoMapper necesita saber cómo convertir el hijo "CreateDto" a la entidad "DetalleOrdenCompra"
            CreateMap<DetalleOrdenCompraCreateDto, DetalleOrdenCompra>()
                .ForMember(dest => dest.IdDetalleRequerimiento, opt => opt.MapFrom(src => src.IdDetalleRequerimiento)) // ¡Vital para Trazabilidad!
                .ForMember(dest => dest.ImporteTotal, opt => opt.Ignore())      // Lo calculamos en el Servicio
                .ForMember(dest => dest.CantidadRecibida, opt => opt.Ignore()); // Empieza en 0 por defecto

            // 2.1 Mapeos auxiliares antiguos (Entidad <-> DTO Lectura)
            CreateMap<DetalleOrdenCompraDto, DetalleOrdenCompra>();
            CreateMap<DetalleOrdenCompra, DetalleOrdenCompraDto>()
              .ForMember(dest => dest.Material, opt => opt.MapFrom(src => src.Material.Nombre)) // Coincide con public string Material
              .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src => src.Material.UnidadMedida)) // Coincide con public string UnidadMedida
              .ForMember(dest => dest.ImporteTotal, opt => opt.MapFrom(src => src.ImporteTotal));

            // 3. Mapeo para la Creación (DTO -> Entidad Padre)
            CreateMap<OrdenCompraCreateDto, OrdenCompra>()
                .ForMember(dest => dest.RequerimientoId, opt => opt.MapFrom(src => src.RequerimientoId)) // ¡AÑADIR ESTO!
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles))
                .ForMember(dest => dest.ProyectoId, opt => opt.MapFrom(src => src.ProyectoId))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => "Pendiente"))
                .ForMember(dest => dest.SubTotal, opt => opt.Ignore())
                .ForMember(dest => dest.Impuesto, opt => opt.Ignore())
                .ForMember(dest => dest.Total, opt => opt.Ignore());

            // 4. Mapeos para Recepción (Modal)
            CreateMap<DetalleOrdenCompra, RecepcionDetalleDto>()
                .ForMember(dest => dest.DetalleOrdenCompraId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.MaterialId, opt => opt.MapFrom(src => src.IdMaterial))
                .ForMember(dest => dest.MaterialNombre,
                           opt => opt.MapFrom(src => src.Material != null ? src.Material.Nombre : "N/A"))
                .ForMember(dest => dest.UnidadMedida,
                           opt => opt.MapFrom(src => src.Material != null ? src.Material.UnidadMedida : "N/A"))
                .ForMember(dest => dest.CantidadPedida, opt => opt.MapFrom(src => src.Cantidad))
                .ForMember(dest => dest.CantidadYaRecibida, opt => opt.MapFrom(src => src.CantidadRecibida))
                .ForMember(dest => dest.CantidadARecibir, opt => opt.Ignore());

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