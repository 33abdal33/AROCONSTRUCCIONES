using AROCONSTRUCCIONES.Dtos;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IPresupuestoService
    {
        Task<IEnumerable<PartidaDto>> GetPartidasPorProyectoAsync(int proyectoId);

        // El método estrella: Recibe el archivo (stream) y el proyecto destino
        Task ImportarPresupuestoDesdeExcelAsync(Stream fileStream, int proyectoId);

        Task EliminarPresupuestoAsync(int proyectoId); // Para limpiar y volver a cargar
    }
}