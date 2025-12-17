using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IProyectoService
    {
        Task<IEnumerable<ProyectoDto>> GetAllAsync();

        // <--- AGREGAR ESTA LÍNEA (Es lo que busca tu Controlador) ---
        Task<IEnumerable<ProyectoDto>> GetAllProyectosAsync();
        // -----------------------------------------------------------

        Task<ProyectoDto?> GetByIdAsync(int id);
        Task CreateAsync(ProyectoDto dto);
        Task<Proyecto> UpdateAsync(int id, ProyectoDto dto);
    }
}
