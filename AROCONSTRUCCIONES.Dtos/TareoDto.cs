using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class TareoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Se requiere la fecha")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public int ProyectoId { get; set; }
        public string? ProyectoNombre { get; set; }

        public string? Responsable { get; set; }
        public string Estado { get; set; } = "Abierto";

        // La cuadrícula de trabajadores
        public List<DetalleTareoDto> Detalles { get; set; } = new();
    }
}
