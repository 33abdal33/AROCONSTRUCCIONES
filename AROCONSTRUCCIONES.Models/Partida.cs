using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AROCONSTRUCCIONES.Models
{
    public class Partida : EntityBase
    {
        [Required]
        public int ProyectoId { get; set; }
        [ForeignKey("ProyectoId")]
        public Proyecto Proyecto { get; set; }

        [Required]
        [StringLength(20)]
        public string Item { get; set; } // El código jerárquico: "01.01", "02.03.05"

        [Required]
        [StringLength(255)]
        public string Descripcion { get; set; } // "Concreto f'c=210 kg/cm2 en columnas"

        [StringLength(10)]
        public string? Unidad { get; set; } // m3, kg, und, glb

        // --- Datos del Presupuesto (Meta) ---

        [Column(TypeName = "decimal(18,2)")]
        public decimal Metrado { get; set; } // Cantidad presupuestada

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; } // Costo directo unitario

        [Column(TypeName = "decimal(18,2)")]
        public decimal Parcial { get; set; } // Metrado * PrecioUnitario

        // --- Control de Ejecución (Real) ---
        // Aquí iremos acumulando lo que gastamos realmente (Materiales + Mano de Obra)

        [Column(TypeName = "decimal(18,2)")]
        public decimal CostoEjecutado { get; set; } = 0;

        // Indica si es un título (ej: "01. ESTRUCTURAS") o una partida real
        // Los títulos no tienen unidad ni precio, solo suman a los hijos.
        public bool EsTitulo { get; set; } = false;
    }
}