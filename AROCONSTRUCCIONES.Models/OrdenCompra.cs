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
        public string Codigo { get; set; } // Ej: OC-2025-001

        [ForeignKey("Proveedor")]
        public int IdProveedor { get; set; }
        public Proveedor? Proveedor { get; set; }

        public DateTime FechaEmision { get; set; } = DateTime.Now;
        public DateTime? FechaEntregaPactada { get; set; } // ¿Cuándo prometió el proveedor traerlo?

        // MEJORA 4: Manejo de Moneda (Vital en construcción)
        public string Moneda { get; set; } = "PEN"; // "PEN" o "USD"
        public decimal TipoCambio { get; set; } = 1; // Si es USD, guardas el TC del día

        // MEJORA 5: Desglose de Impuestos (Robustez contable)
        public decimal SubTotal { get; set; }
        public decimal Impuesto { get; set; } // IGV (18%)
        public decimal Total { get; set; }

        public string Estado { get; set; } = "Emitida"; // Emitida, Aprobada, Anulada, Recepcionada

        // MEJORA 6: Condiciones de pago
        public string FormaPago { get; set; } // "Contado", "Crédito 30 días", etc.

        public string? Observaciones { get; set; }
        public string? RutaPdf { get; set; }

        public int? ProyectoId { get; set; }
        public Proyecto? Proyecto { get; set; }

        public ICollection<DetalleOrdenCompra>? Detalles { get; set; }
    }
}
