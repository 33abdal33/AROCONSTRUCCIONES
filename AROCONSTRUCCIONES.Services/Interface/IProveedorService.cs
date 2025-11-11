using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models; // <-- Añadido
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IProveedorService
    {
        // Lectura
        Task<IEnumerable<ProveedorDto>> GetAllActiveAsync();
        Task<IEnumerable<ProveedorDto>> GetAllAsync(); // Para la lista (incluye inactivos)
        Task<ProveedorDto?> GetByIdAsync(int id);

        // --- REFACTORIZADO ---
        // 'Create' ya no devuelve nada
        Task CreateAsync(ProveedorDto dto);

        // 'Update' devuelve la entidad para que el controlador la tenga
        Task<Proveedor> UpdateAsync(int id, ProveedorDto dto);

        // 'Delete' se renombra a 'Deactivate' (Soft Delete)
        Task<bool> DeactivateAsync(int id);

        // 1. Para OBTENER los datos del modal (ambas pestañas)
        Task<ProveedorEdicionViewModelDto> GetEdicionProveedorAsync(int id);

        // 2. Para GUARDAR los datos del modal (ambas pestañas)
        Task UpdateProveedorCompletoAsync(ProveedorEdicionViewModelDto vm);
    }
}