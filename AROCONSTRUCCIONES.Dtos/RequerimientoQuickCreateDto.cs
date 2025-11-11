using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class RequerimientoQuickCreateDto
    {
        [Required]
        public int IdProyecto { get; set; }

        [Required(ErrorMessage = "El solicitante es obligatorio.")]
        public string Solicitante { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La prioridad es obligatoria.")]
        public string Prioridad { get; set; }

        public string? Observaciones { get; set; }
    }
}