using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AROCONSTRUCCIONES.Models
{
    public class DetalleSolicitudPago : EntityBase
    {
        public int SolicitudPagoId { get; set; }
        [ForeignKey("SolicitudPagoId")]
        public SolicitudPago SolicitudPago { get; set; }

        // --- DATOS DEL DOCUMENTO ---
        [Required]
        [StringLength(10)]
        public string TipoDocumento { get; set; } // OC, FT, RH

        [StringLength(10)]
        public string SerieDocumento { get; set; } // Corregido

        [Required]
        [StringLength(20)]
        public string NumeroDocumento { get; set; }

        public DateTime FechaEmisionDocumento { get; set; } // Corregido

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        // --- VINCULACIÓN ---
        public int? OrdenCompraId { get; set; } // Corregido
        [ForeignKey("OrdenCompraId")]
        public OrdenCompra? OrdenCompra { get; set; }

        [StringLength(250)]
        public string? Observacion { get; set; }
    }
}