using AROCONSTRUCCIONES.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IMovimientoInventarioServices
    {
        Task<bool> RegistrarIngreso(MovimientoInventarioDto dto);
        Task<bool> RegistrarSalida(MovimientoInventarioDto dto);

        // CORRECCIÓN: Debe tener solo UN parámetro (el DTO)
        Task<bool> RealizarTransferenciaAsync(TransferenciaDto dto);

        Task<IEnumerable<MovimientoInventarioDto>> GetAllMovimientosAsync();
        Task<IEnumerable<MovimientoInventarioDto>> GetHistorialPorMaterialYAlmacenAsync(int materialId, int almacenId);
        // Añade esta también para el reporte que haremos
        Task<IEnumerable<ConsumoMaterialProyectoDto>> GetConsumoDetalladoPorProyectoAsync(int proyectoId);
    }
}
