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
        Task CreateAsync(ProveedorEdicionViewModelDto vm); // <-- Ahora

        // 'Update' devuelve la entidad para que el controlador la tenga
        Task<Proveedor> UpdateAsync(int id, ProveedorDto dto);

        Task<bool> DeactivateAsync(int id);

        // --- ESTAS SE QUEDAN IGUAL ---
        Task<ProveedorEdicionViewModelDto> GetEdicionProveedorAsync(int id);
        Task UpdateProveedorCompletoAsync(ProveedorEdicionViewModelDto vm);
    }
}