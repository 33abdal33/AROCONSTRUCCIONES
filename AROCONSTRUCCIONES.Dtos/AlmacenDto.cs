using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class AlmacenDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del almacén es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no debe exceder los 100 caracteres.")]
        [Display(Name = "Nombre del Almacén")]
        public string Nombre { get; set; }

        [StringLength(200, ErrorMessage = "La ubicación no debe exceder los 200 caracteres.")]
        [Display(Name = "Ubicación")]
        public string? Ubicacion { get; set; }

        [StringLength(100, ErrorMessage = "El responsable no debe exceder los 100 caracteres.")]
        public string? Responsable { get; set; }

        [Display(Name = "¿Está Activo?")]
        public bool Estado { get; set; } = true;
    }
}