using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class Tareo : EntityBase
    {
        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public int ProyectoId { get; set; }
        [ForeignKey("ProyectoId")]
        public Proyecto Proyecto { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Abierto"; // Abierto, Cerrado (Procesado en Planilla)

        [StringLength(100)]
        public string? Responsable { get; set; } // Quién llenó el tareo (Capataz/Ingeniero)

        public ICollection<DetalleTareo> Detalles { get; set; } = new List<DetalleTareo>();
    }
}
