using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class OrdenCompraCreateDto
    {
        [Required(ErrorMessage = "El código es obligatorio")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un proveedor")]
        [Display(Name = "Proveedor")]
        public int IdProveedor { get; set; }

        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public string? Observaciones { get; set; }

        // ¡Clave! Lista de detalles que vendrán desde el formulario (probablemente vía JS)
        public List<DetalleOrdenCompraDto> Detalles { get; set; } = new();
    }
}
