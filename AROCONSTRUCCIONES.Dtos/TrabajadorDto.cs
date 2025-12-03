using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class TrabajadorDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [StringLength(15, ErrorMessage = "Máximo 15 caracteres")]
        public string NumeroDocumento { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellidos { get; set; }

        public string NombreCompleto => $"{Apellidos}, {Nombres}";

        [Required(ErrorMessage = "Debe seleccionar un cargo")]
        public int CargoId { get; set; }
        public string? CargoNombre { get; set; } // Para mostrar en la lista

        [Required(ErrorMessage = "Seleccione un sistema de pensión")]
        public string SistemaPension { get; set; } // ONP, INTEGRA, etc.

        public string? Cuspp { get; set; }
        public string? Telefono { get; set; }

        [Display(Name = "Banco")]
        public string? Banco { get; set; }

        [Display(Name = "Nro. Cuenta")]
        public string? NumeroCuenta { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        public bool Estado { get; set; } = true;
    }
}
