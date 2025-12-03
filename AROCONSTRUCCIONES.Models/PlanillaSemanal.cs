using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class PlanillaSemanal : EntityBase
    {
        [Required]
        [StringLength(20)]
        public string Codigo { get; set; } // Ej: PL-2025-WK48-PROY1

        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public int ProyectoId { get; set; }
        [ForeignKey("ProyectoId")]
        public Proyecto Proyecto { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Borrador"; // Borrador, Aprobado, Pagado

        // Totales para la Solicitud de Pago
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBruto { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDescuentos { get; set; } // ONP/AFP + Conafovicer

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalNetoAPagar { get; set; } // Lo que sale de caja

        public ICollection<DetallePlanilla> Detalles { get; set; } = new List<DetallePlanilla>();
    }
}
