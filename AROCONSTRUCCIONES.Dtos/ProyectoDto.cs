using System.ComponentModel.DataAnnotations;

namespace AROCONSTRUCCIONES.Dtos
{
    public class ProyectoDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código es obligatorio")]
        [Display(Name = "Código de Proyecto")]
        public string CodigoProyecto { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre del Proyecto")]
        public string NombreProyecto { get; set; }

        [Display(Name = "Nombre del Cliente")]
        public string? NombreCliente { get; set; }

        public string? Ubicacion { get; set; }
        public string? Responsable { get; set; }

        [Display(Name = "Avance (%)")]
        [Range(0, 100, ErrorMessage = "El avance debe estar entre 0 y 100")]
        public int AvancePorcentaje { get; set; } = 0;

        [Display(Name = "Presupuesto (S/.)")]
        public decimal Presupuesto { get; set; } = 0;

        [Display(Name = "Costo Ejecutado (S/.)")]
        public decimal CostoEjecutado { get; set; } = 0; // Este campo lo llenará el sistema

        [Required(ErrorMessage = "El estado es obligatorio")]
        public string Estado { get; set; } = "Planificación";

        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }
    }
}