using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AROCONSTRUCCIONES.Dtos
{
    // Este DTO/ViewModel lleva TODOS los datos que el modal necesita
    public class ProveedorEdicionViewModelDto
    {
        // Pestaña 1: Datos Generales
        public ProveedorDto Proveedor { get; set; }

        // --- ¡CAMPOS NUEVOS PARA EL NUEVO DISEÑO! ---

        // Para el dropdown de "Categoría"
        public List<string> CategoriasMateriales { get; set; }

        // Para la lista de checkboxes de "Materiales que Suministra"
        public List<MaterialDto> TodosLosMateriales { get; set; }

        // Los IDs de los materiales que SÍ están asignados
        public List<int> MaterialesAsignadosIds { get; set; }

        public ProveedorEdicionViewModelDto()
        {
            Proveedor = new ProveedorDto { Estado = true };
            CategoriasMateriales = new List<string>();
            TodosLosMateriales = new List<MaterialDto>();
            MaterialesAsignadosIds = new List<int>();
        }
    }
}