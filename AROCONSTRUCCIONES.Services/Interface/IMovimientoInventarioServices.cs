using AROCONSTRUCCIONES.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IMovimientoInventarioServices
    {
        Task<bool> RegistrarIngreso(MovimientoInventarioDto dto);
        Task<bool> RegistrarSalida(MovimientoInventarioDto dto);
        Task<IEnumerable<MovimientoInventarioDto>> GetAllMovimientosAsync();

        // ============================================
        // ↓↓↓ ¡AÑADE ESTA LÍNEA NUEVA! ↓↓↓
        // ============================================
        Task<IEnumerable<MovimientoInventarioDto>> GetHistorialPorMaterialYAlmacenAsync(int materialId, int almacenId);
    }
}