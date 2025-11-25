using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AROCONSTRUCCIONES.Models
{
    public class SolicitudPago : EntityBase
    {
        [Required]
        [StringLength(50)] // Aumentado para evitar el error de truncado
        public string Codigo { get; set; }

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;
        public DateTime? FechaAutorizacion { get; set; }
        public DateTime? FechaPago { get; set; }

        // --- ¡PROPIEDAD AGREGADA AQUÍ! ---
        [Required]
        [StringLength(20)]
        public string Moneda { get; set; } = "NUEVOS SOLES";
        // ---------------------------------

        // Estados: Pendiente, Autorizado, Pagado, Anulado
        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente";

        // --- RELACIONES ---
        public int ProyectoId { get; set; }
        [ForeignKey("ProyectoId")]
        public Proyecto Proyecto { get; set; }

        public int ProveedorId { get; set; }
        [ForeignKey("ProveedorId")]
        public Proveedor Proveedor { get; set; }

        public int? OrdenCompraId { get; set; }
        [ForeignKey("OrdenCompraId")]
        public OrdenCompra? OrdenCompra { get; set; }

        // --- SNAPSHOT: DATOS DEL BENEFICIARIO (Histórico) ---
        [StringLength(200)]
        public string? BeneficiarioNombre { get; set; }

        [StringLength(20)]
        public string? BeneficiarioRUC { get; set; }

        [StringLength(50)]
        public string? Banco { get; set; }

        [StringLength(50)]
        public string? NumeroCuenta { get; set; }

        [StringLength(50)]
        public string? CCI { get; set; }

        // --- TOTALES ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoAmortizar { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal FondoGarantia { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MontoNetoAPagar { get; set; }

        [StringLength(500)]
        public string? Concepto { get; set; }

        // --- AUDITORÍA ---
        public string? SolicitadoPorUserId { get; set; }
        [ForeignKey("SolicitadoPorUserId")]
        public ApplicationUser? SolicitadoPorUser { get; set; }

        public string? AutorizadoPorUserId { get; set; }
        [ForeignKey("AutorizadoPorUserId")]
        public ApplicationUser? AutorizadoPorUser { get; set; }

        public ICollection<DetalleSolicitudPago> Detalles { get; set; } = new List<DetalleSolicitudPago>();
    }
}