using AROCONSTRUCCIONES.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IRecursosHumanosService
    {
        Task<IEnumerable<TrabajadorDto>> GetAllTrabajadoresAsync();
        Task<TrabajadorDto?> GetTrabajadorByIdAsync(int id);
        Task CreateTrabajadorAsync(TrabajadorDto dto);
        Task UpdateTrabajadorAsync(int id, TrabajadorDto dto);
        Task<IEnumerable<CargoDto>> GetAllCargosAsync();
        Task<TareoDto> GetTareoParaEditarAsync(int id, int proyectoId);
        Task GuardarTareoAsync(TareoDto dto);
        Task<IEnumerable<TareoDto>> GetHistorialTareosAsync(int proyectoId);
        Task<PlanillaSemanalDto> GenerarPrePlanillaAsync(int proyectoId, DateTime inicio, DateTime fin);
        Task GuardarPlanillaAsync(PlanillaSemanalDto dto);
        Task<IEnumerable<PlanillaSemanalDto>> GetHistorialPlanillasAsync(int proyectoId);
    }
}
