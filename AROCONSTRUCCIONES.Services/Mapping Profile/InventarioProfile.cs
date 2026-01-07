using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AutoMapper;

namespace AROCONSTRUCCIONES.Services.Mapping_Profile
{
    public class InventarioProfile : Profile
    {
        public InventarioProfile()
        {
            CreateMap<Inventario, InventarioDto>()
                // 1. Mapeo de IDs base
                .ForMember(dest => dest.MaterialId, opt => opt.MapFrom(src => src.MaterialId))
                .ForMember(dest => dest.AlmacenId, opt => opt.MapFrom(src => src.AlmacenId))

                // 2. LA CLAVE: Mapeo del Proyecto desde el Almacén (Para el Filtro)
                // Dentro de InventarioProfile.cs
                .ForMember(dest => dest.ProyectoId,
                    opt => opt.MapFrom(src => src.Almacen != null ? src.Almacen.ProyectoId : (int?)null))

                // 3. Mapeo de datos del Material
                .ForMember(dest => dest.MaterialCodigo,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.Codigo : string.Empty))
                .ForMember(dest => dest.MaterialNombre,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.Nombre : "Material No Encontrado"))
                .ForMember(dest => dest.MaterialCategoria,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.Categoria : string.Empty))
                .ForMember(dest => dest.MaterialUnidadMedida,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.UnidadMedida : string.Empty))
                .ForMember(dest => dest.MaterialPrecioUnidad,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.PrecioUnitario : 0))

                // 4. Mapeo de datos del Almacén
                .ForMember(dest => dest.AlmacenNombre,
                    opt => opt.MapFrom(src => src.Almacen != null ? src.Almacen.Nombre : "Sin Nombre"))
                .ForMember(dest => dest.AlmacenUbicacion,
                    opt => opt.MapFrom(src => src.Almacen != null ? src.Almacen.Ubicacion : "Sin Ubicación"))

                // 5. Datos de Inventario
                .ForMember(dest => dest.StockActual, opt => opt.MapFrom(src => src.StockActual))
                .ForMember(dest => dest.StockMinimo, opt => opt.MapFrom(src => src.StockMinimo))
                .ForMember(dest => dest.CostoPromedio, opt => opt.MapFrom(src => src.CostoPromedio))
                .ForMember(dest => dest.NivelAlerta, opt => opt.MapFrom(src => src.NivelAlerta))
                .ForMember(dest => dest.FechaUltimoMovimiento, opt => opt.MapFrom(src => src.FechaUltimoMovimiento))

                // 6. Ignorar propiedades calculadas en el DTO (se calculan solas allá)
                .ForMember(dest => dest.ValorTotal, opt => opt.Ignore())
                .ForMember(dest => dest.EstadoTexto, opt => opt.Ignore());
        }
    }
}