using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AROCONSTRUCCIONES.Models
{
    public class MovimientoBancario : EntityBase
    {
        public int CuentaBancariaId { get; set; }
        [ForeignKey("CuentaBancariaId")]
        public CuentaBancaria CuentaBancaria { get; set; }

        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        [Required]
        [StringLength(20)]
        public string TipoMovimiento { get; set; } // "INGRESO" o "EGRESO"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoDespues { get; set; } // Snapshot del saldo

        [StringLength(255)]
        public string? Descripcion { get; set; } // Ej: "Pago Planilla Sem 48"

        [StringLength(50)]
        public string? NumeroOperacion { get; set; } // Voucher

        // Referencia opcional a la Solicitud de Pago
        public int? SolicitudPagoId { get; set; }
        [ForeignKey("SolicitudPagoId")]
        public SolicitudPago? SolicitudPago { get; set; }
    }
}