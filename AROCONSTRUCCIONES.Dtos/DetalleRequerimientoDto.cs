using System.ComponentModel.DataAnnotations;

namespace AROCONSTRUCCIONES.Dtos
{
    public class DetalleRequerimientoDto
    {
        [Required]
        public int IdMaterial { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        // Nota: No hay precio. El almacén asignará el costo al momento del despacho.
    }
}
