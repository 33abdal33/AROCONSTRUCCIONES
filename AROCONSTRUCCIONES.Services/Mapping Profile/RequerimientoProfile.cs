using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AutoMapper;
using System;

namespace AROCONSTRUCCIONES.Services.Mapping_Profile
{
    public class RequerimientoProfile : Profile
    {
        public RequerimientoProfile()
        {
            // ... (Mapeo 1 y 2 sin cambios) ...
            CreateMap<DetalleRequerimientoDto, DetalleRequerimiento>()
                .ForMember(dest => dest.CantidadSolicitada, opt => opt.MapFrom(src => src.Cantidad));

            CreateMap<RequerimientoCreateDto, Requerimiento>()
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles))
                .ForMember(dest => dest.Proyecto, opt => opt.Ignore());

            CreateMap<RequerimientoQuickCreateDto, Requerimiento>()
               .ForMember(dest => dest.Detalles, opt => opt.Ignore())
               .ForMember(dest => dest.Proyecto, opt => opt.Ignore())
               .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => $"REQ-{DateTime.Now:yyyyMMdd-HHmmss}"));

            // --- MAPEO 3 (Lista Global) ---
            // (Este ya era seguro)
            CreateMap<Requerimiento, RequerimientoListDto>()
                .ForMember(dest => dest.ProyectoNombre,
                           opt => opt.MapFrom(src => src.Proyecto != null ? src.Proyecto.NombreProyecto : "N/A"));

            // --- MAPEO 4 (Detalles Materiales) ---
            // (Este es seguro porque asume que Material nunca es nulo, lo cual es correcto)
            CreateMap<DetalleRequerimiento, DetalleRequerimientoDetailsDto>()
                .ForMember(dest => dest.MaterialCodigo, opt => opt.MapFrom(src => src.Material.Codigo))
                .ForMember(dest => dest.MaterialNombre, opt => opt.MapFrom(src => src.Material.Nombre))
                .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src => src.Material.UnidadMedida));


            // --- ¡¡CORRECCIÓN AQUÍ!! ---
            // --- MAPEO 5: Para el modal de DETALLES (Maestro) ---
            CreateMap<Requerimiento, RequerimientoDetailsDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id)) // <-- AÑADIR/ASEGURAR ESTA LÍNEA
                .ForMember(dest => dest.ProyectoNombre,
                           opt => opt.MapFrom(src => src.Proyecto != null ? src.Proyecto.NombreProyecto : "Proyecto no asignado"))
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));

            // 6. Mapeo de DetalleRequerimiento -> DetalleOrdenCompraDto
            // (Usado para pre-llenar la tabla de la OC)
            CreateMap<DetalleRequerimiento, DetalleOrdenCompraDto>()
                .ForMember(dest => dest.IdMaterial, opt => opt.MapFrom(src => src.IdMaterial))
                .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.CantidadSolicitada - src.CantidadAtendida)) // Solo lo pendiente
                .ForMember(dest => dest.PrecioUnitario, opt => opt.Ignore()); // El comprador debe poner el precio

            // 7. Mapeo de Requerimiento -> OrdenCompraCreateDto
            // (El mapeo principal para pre-llenar el modal)
            CreateMap<Requerimiento, OrdenCompraCreateDto>()
                .ForMember(dest => dest.Codigo, opt => opt.MapFrom(src => $"OC-REQ-{src.Codigo}")) // Sugiere un código
                .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => $"Basado en Requerimiento: {src.Codigo}"))
                .ForMember(dest => dest.IdProveedor, opt => opt.Ignore()) // El comprador debe elegirlo
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles.Where(d => d.CantidadSolicitada > d.CantidadAtendida)));
        }
    }
}