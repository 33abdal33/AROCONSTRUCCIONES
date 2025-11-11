using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models; // Asumiendo que esta es tu entidad 'Inventario'
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Mapping_Profile
{
    public class InventarioProfile : Profile
    {
        public InventarioProfile()
        {
            CreateMap<Inventario, InventarioDto>()

                // --- CORRECCIÓN DEL ERROR DE COMPILACIÓN ("Cantidad") ---
                // Mapeo 1: StockActual (Si bien es el mismo nombre, es bueno explicitar)
                // No uses .MapFrom(src => src.Cantidad), usa el nombre real:
                .ForMember(dest => dest.StockActual, opt => opt.MapFrom(src => src.StockActual))

                // ⭐ ELIMINAR ESTA LÍNEA DE MAPEO EXPLÍCITO DE STOCK MÍNIMO ⭐
                // Ya que ambas clases (Inventario y InventarioDto) tienen la propiedad StockMinimo,
                // AutoMapper la mapeará automáticamente por convención.
                // .ForMember(dest => dest.StockMinimo, opt => opt.MapFrom(src => src.Material != null ? src.Material.StockMinimo : 0))

                // --- Mapeo de Propiedades Anidadas (Material) ---
                .ForMember(dest => dest.MaterialCodigo,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.Codigo : string.Empty))
                .ForMember(dest => dest.MaterialNombre,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.Nombre : "Material No Encontrado"))
                .ForMember(dest => dest.MaterialCategoria,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.Categoria : string.Empty))
                .ForMember(dest => dest.MaterialUnidadMedida,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.UnidadMedida : string.Empty))
                .ForMember(dest => dest.MaterialPrecioUnidad,
                    opt => opt.MapFrom(src => src.Material != null ? src.Material.PrecioUnitario : decimal.MinValue))

                // --- Mapeo de Propiedades Anidadas (Almacén) ---
                .ForMember(dest => dest.AlmacenUbicacion,
                    opt => opt.MapFrom(src =>
                        src.Almacen != null
                        ? $"{src.Almacen.Nombre} - {src.Almacen.Ubicacion}"
                        : "Almacén No Encontrado"))

                // --- Propiedades Calculadas y de Alerta ---
                // Puedes añadir aquí el mapeo de NivelAlerta si es necesario (el nombre coincide, debería ser automático)
                .ForMember(dest => dest.NivelAlerta, opt => opt.MapFrom(src => src.NivelAlerta))

                // --- Ignorar Propiedades Calculadas en el DTO (Buena Práctica) ---
                .ForMember(dest => dest.ValorTotal, opt => opt.Ignore())
                .ForMember(dest => dest.EstadoTexto, opt => opt.Ignore());
        }
    }
}