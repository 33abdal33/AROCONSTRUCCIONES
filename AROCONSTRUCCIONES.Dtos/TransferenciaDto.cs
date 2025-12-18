using System.ComponentModel.DataAnnotations;

namespace AROCONSTRUCCIONES.Dtos
{
    public class TransferenciaDto
    {
        [Required(ErrorMessage = "El almacén de origen es obligatorio")]
        public int AlmacenOrigenId { get; set; }

        [Required(ErrorMessage = "El almacén de destino es obligatorio")]
        public int AlmacenDestinoId { get; set; }

        [Required(ErrorMessage = "El material es obligatorio")]
        public int MaterialId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        public string? Observacion { get; set; } // Ej: "Traslado para vaciado de techo"
        // Este campo lo llenamos en el controlador, no el usuario
        public string? ResponsableNombre { get; set; }
    }
}