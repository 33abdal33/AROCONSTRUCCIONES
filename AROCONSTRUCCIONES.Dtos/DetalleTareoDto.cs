using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class DetalleTareoDto
    {
        public int Id { get; set; } // Id del detalle (si existe)
        public int TrabajadorId { get; set; }
        public string TrabajadorNombre { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty; // Para mostrar "Operario", "Peón"

        // Inputs del usuario
        public decimal HorasNormales { get; set; } = 8;
        public decimal HorasExtras60 { get; set; } = 0;
        public decimal HorasExtras100 { get; set; } = 0;
        public bool Asistio { get; set; } = true;
        public string TipoAsistencia { get; set; } = "LB"; // Para la vista
    }
}
