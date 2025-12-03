using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class ComprobantePago : EntityBase
    {
        // --- Clasificación ---
        [Required]
        [StringLength(20)]
        public string TipoComprobante { get; set; } // FT (Factura), RH (Recibo Honorarios), BV (Boleta), LC (Letra)

        [Required]
        [StringLength(10)]
        public string Serie { get; set; }

        [Required]
        [StringLength(20)]
        public string Numero { get; set; }

        // --- Fechas ---
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; } // Para Programación de Pagos
        public DateTime FechaContable { get; set; }    // Para el mes de declaración (DAOT/Renta)

        // --- Relaciones ---
        public int ProveedorId { get; set; }
        [ForeignKey("ProveedorId")]
        public Proveedor Proveedor { get; set; }

        public int? OrdenCompraId { get; set; } // Vinculación opcional si viene de Logística
        [ForeignKey("OrdenCompraId")]
        public OrdenCompra? OrdenCompra { get; set; }

        // --- Importes ---
        public string Moneda { get; set; } = "PEN";
        public decimal TipoCambio { get; set; } = 1.0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; } // Base Imponible

        [Column(TypeName = "decimal(18,2)")]
        public decimal IGV { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NoGravado { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // --- Impuestos y Retenciones (Tus requerimientos) ---
        public bool TieneDetraccion { get; set; }
        public decimal PorcentajeDetraccion { get; set; } // Ej: 4%, 10%, 12%
        public decimal MontoDetraccion { get; set; }
        public string? NumeroConstanciaDetraccion { get; set; }
        public DateTime? FechaPagoDetraccion { get; set; }

        public decimal MontoRetencion { get; set; } // Renta de 4ta o IGV

        // --- Estado de Pago ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoPendiente { get; set; } // Cuánto falta pagar de esta factura

        [StringLength(20)]
        public string EstadoPago { get; set; } = "Pendiente"; // Pendiente, Parcial, Pagado, Anulado

        public string? Glosa { get; set; } // Descripción breve para contabilidad
    }
}
