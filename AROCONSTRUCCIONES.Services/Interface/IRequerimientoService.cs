using AROCONSTRUCCIONES.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IRequerimientoService
    {
        // Obtiene la lista para la pestaña del proyecto
        Task<IEnumerable<RequerimientoListDto>> GetRequerimientosPorProyectoAsync(int proyectoId);

        // Lógica para crear el nuevo requerimiento (Maestro-Detalle)
        Task<IEnumerable<RequerimientoListDto>> GetAllRequerimientosAsync();
        // Volvemos a usar el DTO completo que incluye la lista de Detalles
        Task CreateAsync(RequerimientoCreateDto dto); // Antes: RequerimientoQuickCreateDto
        Task<RequerimientoDetailsDto> GetRequerimientoDetailsAsync(int id);
    }
}
