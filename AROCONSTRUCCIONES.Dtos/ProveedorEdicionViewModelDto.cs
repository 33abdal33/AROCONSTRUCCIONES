namespace AROCONSTRUCCIONES.Dtos
{
    public class ProveedorEdicionViewModelDto
    {
        // Datos principales del proveedor (incluye ahora los campos bancarios)
        public ProveedorDto Proveedor { get; set; }

        // Para el dropdown de especialidades/categorías
        public List<string> CategoriasMateriales { get; set; }

        // Lista completa de materiales para los checkboxes
        public List<MaterialDto> TodosLosMateriales { get; set; }

        // IDs de los materiales que el proveedor ya tiene asignados
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