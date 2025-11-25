using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class OrdenCompra : EntityBase
    {
        public string Codigo { get; set; }
        public int IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Pendiente";
        public decimal Total { get; set; } = 0;
        public string? Observaciones { get; set; }
        // --- ¡AÑADE ESTA LÍNEA! ---
        public string? RutaPdf { get; set; } // Guardará algo como "/ordenes_pdf/OC-2025-001.pdf"
        public int? ProyectoId { get; set; } // Puede ser nulo si es una compra de stock general
        [ForeignKey("ProyectoId")]
        public Proyecto? Proyecto { get; set; }
        public ICollection<DetalleOrdenCompra>? Detalles { get; set; }
    }
}
