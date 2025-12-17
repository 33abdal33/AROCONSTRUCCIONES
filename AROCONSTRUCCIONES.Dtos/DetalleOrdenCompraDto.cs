using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class DetalleOrdenCompraDto
    {
        public int Id { get; set; } // Útil si necesitas editar/borrar líneas luego
        public int IdMaterial { get; set; }

        // --- CAMPOS NUEVOS (Solucionan el error) ---
        // Estos campos recibirán el texto desde la Base de Datos para mostrarlo
        public string? Material { get; set; }      // El nombre del material
        public string? UnidadMedida { get; set; }  // La unidad (UND, BLS, KG)
        // ------------------------------------------

        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        // Cambiamos la propiedad calculada por la real de la BD
        // para asegurar que coincida con lo que se guardó financieramente
        public decimal ImporteTotal { get; set; }
    }
}