using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class DetallePlanilla : EntityBase
    {
        public int PlanillaSemanalId { get; set; }
        [ForeignKey("PlanillaSemanalId")]
        public PlanillaSemanal PlanillaSemanal { get; set; }

        public int TrabajadorId { get; set; }
        [ForeignKey("TrabajadorId")]
        public Trabajador Trabajador { get; set; }

        // --- Resumen de Asistencia ---
        public int DiasTrabajados { get; set; }
        public decimal TotalHorasNormales { get; set; }
        public decimal TotalHorasExtras60 { get; set; }
        public decimal TotalHorasExtras100 { get; set; }

        // --- Cálculo Monetario ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal JornalPromedio { get; set; } // Informativo

        [Column(TypeName = "decimal(18,2)")]
        public decimal SueldoBasico { get; set; } // Horas * Jornal

        [Column(TypeName = "decimal(18,2)")]
        public decimal PagoHorasExtras { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BonificacionBUC { get; set; } // 30% o 32% del básico

        [Column(TypeName = "decimal(18,2)")]
        public decimal Movilidad { get; set; } // Pasajes (S/ 7.20 x día asistido aprox)

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBruto { get; set; }

        // --- Descuentos ---
        public string SistemaPension { get; set; } // ONP / AFP INTEGRA...

        [Column(TypeName = "decimal(18,2)")]
        public decimal AportePension { get; set; } // El monto descontado (aprox 13%)

        [Column(TypeName = "decimal(18,2)")]
        public decimal Conafovicer { get; set; } // 2% del Básico (Obligatorio en construcción)

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDescuentos { get; set; }

        // --- Final ---
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetoAPagar { get; set; }
    }
}
