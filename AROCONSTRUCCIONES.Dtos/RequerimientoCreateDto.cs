using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class RequerimientoCreateDto
    {
        [Required(ErrorMessage = "Debe seleccionar un proyecto.")]
        [Display(Name = "Proyecto")]
        public int IdProyecto { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "El solicitante es obligatorio.")]
        public string? Solicitante { get; set; } // Ej: "Ing. Residente"

        public string? Area { get; set; } // Ej: "Frente 1 - Estructuras"

        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; } = DateTime.Now;

        // La lista de materiales que se enviará desde la tabla
        public List<DetalleRequerimientoDto> Detalles { get; set; }

        public RequerimientoCreateDto()
        {
            Detalles = new List<DetalleRequerimientoDto>();
        }
    }
}
