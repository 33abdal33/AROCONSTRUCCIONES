using System.ComponentModel.DataAnnotations;

namespace AROCONSTRUCCIONES.Dtos
{
    public class OrdenCompraCreateDto
    {
        // Nota: A veces el código se genera automático en el backend, 
        // pero si lo pides en el formulario, mantén el [Required].
        public string? Codigo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un proveedor")]
        [Display(Name = "Proveedor")]
        public int IdProveedor { get; set; }

        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public DateTime? FechaEntregaPactada { get; set; } // <-- Nuevo

        [Required(ErrorMessage = "La moneda es obligatoria")]
        public string Moneda { get; set; } = "PEN"; // <-- Nuevo (Soluciona Error CS1061)

        public string FormaPago { get; set; } = "Contado"; // <-- Nuevo
        public string? Observaciones { get; set; }

        public int? ProyectoId { get; set; }

        // CAMBIO IMPORTANTE: Usamos 'DetalleOrdenCompraCreateDto', no el DTO de lectura
        public List<DetalleOrdenCompraCreateDto> Detalles { get; set; } = new();
    }
}