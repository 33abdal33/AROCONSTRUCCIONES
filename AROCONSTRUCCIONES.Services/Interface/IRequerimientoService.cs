using AROCONSTRUCCIONES.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IRequerimientoService
    {
        Task<IEnumerable<RequerimientoListDto>> GetRequerimientosPorProyectoAsync(int proyectoId);
        Task<IEnumerable<RequerimientoListDto>> GetAllRequerimientosAsync();
        Task<string> GetNextCodigoAsync();
        Task CreateAsync(RequerimientoCreateDto dto);
        Task<RequerimientoDetailsDto> GetRequerimientoDetailsAsync(int id);
        Task<bool> ApproveAsync(int id);
        Task<IEnumerable<RequerimientoListDto>> GetAllAprobadosAsync();
        Task<bool> CancelAsync(int id);
        Task<bool> CambiarEstadoAsync(int id, string nuevoEstado);

        // --- MÉTODO NUEVO PARA EL DESPACHO (VITAL) ---
        // Este método calcula automáticamente si el estado debe ser "Parcial" o "Despachado"
        // basándose en las cantidades físicas entregadas vs las solicitadas.
        Task ActualizarEstadoSegunAtencionAsync(int requerimientoId);
    }
}