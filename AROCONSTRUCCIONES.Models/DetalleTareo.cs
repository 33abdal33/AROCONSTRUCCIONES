using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class DetalleTareo : EntityBase
    {
        public int TareoId { get; set; }
        [ForeignKey("TareoId")]
        public Tareo Tareo { get; set; }

        public int TrabajadorId { get; set; }
        [ForeignKey("TrabajadorId")]
        public Trabajador Trabajador { get; set; }

        // --- Control de Horas ---
        // Por defecto 8 horas si asistió completo
        public decimal HorasNormales { get; set; } = 8;

        // Horas Extras (Construcción civil suele tener al 60% y 100%)
        public decimal HorasExtras60 { get; set; } = 0;
        public decimal HorasExtras100 { get; set; } = 0;

        // Flag de asistencia (True = Asistió, False = Falta)
        public bool Asistio { get; set; } = true;

        // --- Snapshot de Costo (Importante) ---
        // Guardamos cuánto ganaba el obrero ESE DÍA para que el historial no cambie si le suben el sueldo después.
        [Column(TypeName = "decimal(18,2)")]
        public decimal JornalBasicoDiario { get; set; }

        public string CargoDia { get; set; } // Ej: "Operario" (por si ese día hizo otra labor)
    }
}
