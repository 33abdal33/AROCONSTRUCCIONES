using System.ComponentModel.DataAnnotations;

namespace AROCONSTRUCCIONES.Dtos
{
    public class ProveedorDto
    {
        public int Id { get; set; }

        [Required]
        public string? RazonSocial { get; set; }

        [Required]
        [StringLength(11, MinimumLength = 11)] // Suponiendo RUC peruano/similar de 11 dígitos
        public string? RUC { get; set; }

        public string? Direccion { get; set; }
        public string? Telefono { get; set; }   

        [EmailAddress]
        public string? Email { get; set; }
        public string? NombreContacto { get; set; }
        public string? Especialidad { get; set; }    

        public bool Estado { get; set; }
    }
}
