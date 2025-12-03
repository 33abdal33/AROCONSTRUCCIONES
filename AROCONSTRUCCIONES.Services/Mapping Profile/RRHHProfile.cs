using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AutoMapper;

namespace AROCONSTRUCCIONES.Services.Mapping_Profile
{
    public class RRHHProfile : Profile
    {
        public RRHHProfile()
        {
            // --- CARGOS ---
            CreateMap<Cargo, CargoDto>();

            // --- TRABAJADORES ---
            CreateMap<Trabajador, TrabajadorDto>()
                .ForMember(dest => dest.CargoNombre, opt => opt.MapFrom(src => src.Cargo.Nombre));

            CreateMap<TrabajadorDto, Trabajador>()
                .ForMember(dest => dest.Cargo, opt => opt.Ignore());

            // --- TAREO ---
            CreateMap<DetalleTareo, DetalleTareoDto>()
                .ForMember(dest => dest.TrabajadorNombre, opt => opt.MapFrom(src => src.Trabajador.NombreCompleto))
                .ForMember(dest => dest.Cargo, opt => opt.MapFrom(src => src.CargoDia));

            CreateMap<DetalleTareoDto, DetalleTareo>()
                .ForMember(dest => dest.Trabajador, opt => opt.Ignore());

            CreateMap<Tareo, TareoDto>()
                .ForMember(dest => dest.ProyectoNombre, opt => opt.MapFrom(src => src.Proyecto.NombreProyecto));

            CreateMap<TareoDto, Tareo>();

            // --- PLANILLAS (¡ESTO ES LO QUE FALTABA!) ---

            // 1. Detalle Planilla (Fila)
            CreateMap<DetallePlanilla, DetallePlanillaDto>()
                .ForMember(dest => dest.TrabajadorNombre, opt => opt.MapFrom(src => src.Trabajador.NombreCompleto))
                // Si el cargo no se guardó en el detalle, lo sacamos del trabajador
                .ForMember(dest => dest.Cargo, opt => opt.MapFrom(src => src.Trabajador.Cargo.Nombre));

            // 2. Cabecera Planilla
            CreateMap<PlanillaSemanal, PlanillaSemanalDto>()
                .ForMember(dest => dest.ProyectoNombre, opt => opt.MapFrom(src => src.Proyecto.NombreProyecto))
                .ForMember(dest => dest.TotalNeto, opt => opt.MapFrom(src => src.TotalNetoAPagar)) // Mapeo importante
                .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.Detalles));
        }
    }
}