using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AutoMapper;

namespace AROCONSTRUCCIONES.Services.Mapping_Profile
{
    public class MovimientoInventarioProfile : Profile
    {
        public MovimientoInventarioProfile()
        {
            // ---------------------------------------------------------------------
            // 1. Mapeo DTO -> Entidad (USADO para RegistrarIngreso/Salida)
            // ---------------------------------------------------------------------
            CreateMap<MovimientoInventarioDto, MovimientoInventario>()
                // Mapeo normal de propiedades escalares y IDs
                .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.Cantidad))
                .ForMember(dest => dest.CostoUnitarioMovimiento, opt => opt.MapFrom(src => src.CostoUnitarioMovimiento))
                .ForMember(dest => dest.MaterialId, opt => opt.MapFrom(src => src.MaterialId))
                .ForMember(dest => dest.AlmacenId, opt => opt.MapFrom(src => src.AlmacenId))
                .ForMember(dest => dest.ProveedorId, opt => opt.MapFrom(src => src.ProveedorId))
                .ForMember(dest => dest.Motivo, opt => opt.MapFrom(src => src.Motivo))

                // ⭐ SOLUCIÓN CLAVE: IGNORAR OBJETOS DE NAVEGACIÓN COMPLETOS
                // Esto asegura que EF Core solo use las IDs, y NO intente insertar 
                // el Material duplicado al agregar un Movimiento.
                .ForMember(dest => dest.Material, opt => opt.Ignore())
                .ForMember(dest => dest.Almacen, opt => opt.Ignore())
                .ForMember(dest => dest.Proveedor, opt => opt.Ignore());


            // ---------------------------------------------------------------------
            // 2. Mapeo Entidad -> DTO (USADO para la vista Kárdex/Listado)
            // ---------------------------------------------------------------------
            CreateMap<MovimientoInventario, MovimientoInventarioDto>()
                // Propiedades de navegación a DTO plano
                .ForMember(dest => dest.MaterialCodigo,
                             opt => opt.MapFrom(src => src.Material != null ? src.Material.Codigo : "N/A"))
                .ForMember(dest => dest.MaterialNombre,
                             opt => opt.MapFrom(src => src.Material != null ? src.Material.Nombre : "Material Eliminado"))
                .ForMember(dest => dest.UnidadMedida,
                             opt => opt.MapFrom(src => src.Material != null ? src.Material.UnidadMedida : "UN"))
                .ForMember(dest => dest.AlmacenNombre,
                             opt => opt.MapFrom(src => src.Almacen != null ? src.Almacen.Nombre : "Almacén Eliminado"))

                // Mapeo de Propiedades del Movimiento
                .ForMember(dest => dest.ResponsableNombre,
                             opt => opt.MapFrom(src => src.Responsable))
                .ForMember(dest => dest.StockFinal,
                             opt => opt.MapFrom(src => src.StockFinal))
                .ForMember(dest => dest.CostoUnitarioMovimiento,
                             opt => opt.MapFrom(src => src.CostoUnitarioMovimiento));
        }
    }
}
