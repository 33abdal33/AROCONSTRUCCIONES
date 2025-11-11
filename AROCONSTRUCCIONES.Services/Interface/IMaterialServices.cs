using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IMaterialServices
    {
        Task<IEnumerable<MaterialDto>> GetAllAsync();
        Task<IEnumerable<MaterialDto>> GetAllActiveAsync();
        Task<List<string>> GetMaterialCategoriesAsync();
        Task<List<string>> GetMaterialUnitsAsync();
        Task<MaterialDto?> GetByIdAsync(int id);

        // 🔹 Crear un nuevo Material (usando un DTO)
        // ============================================
        // ¡AQUÍ ESTÁ LA CORRECCIÓN!
        // Quitamos 'Task<Material>' y lo dejamos solo como 'Task'
        // ============================================
        Task CreateAsync(MaterialDto dto);

        // 🔹 Actualizar un Material existente (Esta ya la corregimos)
        Task<Material> UpdateAsync(int id, MaterialDto dto);

        // 🔹 Desactivar (Soft Delete)
        Task<bool> DeactivateAsync(int id);
        Task<IEnumerable<MaterialDto>> GetMaterialesPorProveedorAsync(int proveedorId);

    }
}