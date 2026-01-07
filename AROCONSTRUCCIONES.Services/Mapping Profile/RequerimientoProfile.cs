using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AutoMapper;
using System;
using System.Linq;

namespace AROCONSTRUCCIONES.Services.Mapping_Profile
{
    public class RequerimientoProfile : Profile
    {
        public RequerimientoProfile()
        {
            // 1. Mapeos de creación
            CreateMap<DetalleRequerimientoDto, DetalleRequerimiento>()
                .ForMember(dest => dest.CantidadSolicitada, opt => opt.MapFrom(src => src.Cantidad));

            CreateMap<RequerimientoCreateDto, Requerimiento>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles))
                .ForMember(dest => dest.Proyecto, opt => opt.Ignore())
                .ForMember(dest => dest.FechaSolicitud, opt => opt.MapFrom(src => src.Fecha));

            CreateMap<RequerimientoQuickCreateDto, Requerimiento>()
               .ForMember(dest => dest.Detalles, opt => opt.Ignore())
               .ForMember(dest => dest.Proyecto, opt => opt.Ignore())
               .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => $"REQ-{DateTime.Now:yyyyMMdd-HHmmss}"));

            // --- MAPEO 3 (Lista Global) - CORREGIDO ---
            CreateMap<Requerimiento, RequerimientoListDto>()
                .ForMember(dest => dest.ProyectoNombre,
                           opt => opt.MapFrom(src => src.Proyecto != null ? src.Proyecto.NombreProyecto : "N/A"))
                // SOLUCIÓN AL ERROR CS1061: Mapeamos la prioridad para que la vista pueda leerla
                .ForMember(dest => dest.Prioridad, opt => opt.MapFrom(src => src.Prioridad ?? "Normal"))
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.FechaSolicitud));

            // --- MAPEO 4 (Detalles Materiales) ---
            CreateMap<DetalleRequerimiento, DetalleRequerimientoDetailsDto>()
                .ForMember(dest => dest.MaterialCodigo, opt => opt.MapFrom(src => src.Material != null ? src.Material.Codigo : "S/C"))
                .ForMember(dest => dest.MaterialNombre, opt => opt.MapFrom(src => src.Material != null ? src.Material.Nombre : "Material No Encontrado"))
                .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src => src.Material != null ? src.Material.UnidadMedida : "Und"));

            // --- MAPEO 5: Para el modal de DETALLES (Maestro) ---
            CreateMap<Requerimiento, RequerimientoDetailsDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProyectoNombre,
                           opt => opt.MapFrom(src => src.Proyecto != null ? src.Proyecto.NombreProyecto : "Proyecto no asignado"))
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles))
                .ForMember(dest => dest.Fecha, opt => opt.MapFrom(src => src.FechaSolicitud));

            // 6. Mapeo de DetalleRequerimiento -> DetalleOrdenCompraDto (Para Compras)
            CreateMap<DetalleRequerimiento, DetalleOrdenCompraDto>()
                .ForMember(dest => dest.IdMaterial, opt => opt.MapFrom(src => src.IdMaterial))
                .ForMember(dest => dest.Material, opt => opt.MapFrom(src => src.Material.Nombre))
                .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.CantidadSolicitada - src.CantidadAtendida))
                .ForMember(dest => dest.PrecioUnitario, opt => opt.Ignore());

            // 7. Mapeo de Requerimiento -> OrdenCompraCreateDto (Para pre-llenar OC)
            CreateMap<Requerimiento, OrdenCompraCreateDto>()
                .ForMember(dest => dest.ProyectoId, opt => opt.MapFrom(src => src.IdProyecto))
                .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => $"OC-REQ-{src.Codigo}"))
                .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => $"Atención de Requerimiento: {src.Codigo}"))
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles.Where(d => d.CantidadSolicitada > d.CantidadAtendida)));
        }
    }
}