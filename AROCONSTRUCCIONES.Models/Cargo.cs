using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class Cargo : EntityBase
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } // Ej: Operario, Oficial, Peón, Capataz

        [Column(TypeName = "decimal(18,2)")]
        public decimal JornalBasico { get; set; } // Pago diario básico (8 horas)

        [Column(TypeName = "decimal(18,2)")]
        public decimal BUC { get; set; } // Bonificación Unificada de Construcción (%)

        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}
