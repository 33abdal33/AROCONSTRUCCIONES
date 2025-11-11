using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models; // <-- Añadido
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IAlmacenService
    {
        // Lectura
        Task<IEnumerable<AlmacenDto>> GetAllAsync();
        Task<AlmacenDto?> GetByIdAsync(int id);
        Task<IEnumerable<AlmacenDto>> GetAllActiveAsync();

        // --- REFACTORIZADO ---
        // 'Create' ya no devuelve nada
        Task CreateAsync(AlmacenDto dto);

        // 'Update' devuelve la entidad
        Task<Almacen> UpdateAsync(int id, AlmacenDto dto);

        // 'Delete' se renombra a 'Deactivate' (Soft Delete)
        Task<bool> DeactivateAsync(int id);
    }
}