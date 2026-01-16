using System.ComponentModel.DataAnnotations;

namespace AROCONSTRUCCIONES.Dtos
{
    public class ProveedorDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La Razón Social es obligatoria")]
        public string? RazonSocial { get; set; }

        [Required(ErrorMessage = "El RUC es obligatorio")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "El RUC debe tener 11 dígitos")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "El RUC solo debe contener números")]
        public string? RUC { get; set; }

        public string? Direccion { get; set; }
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string? Email { get; set; }
        public string? NombreContacto { get; set; }
        public string? Especialidad { get; set; }

        public bool Estado { get; set; }

        // --- NUEVOS CAMPOS FINANCIEROS ---
        [StringLength(50)]
        public string? Banco { get; set; }

        [StringLength(50)]
        public string? NumeroCuenta { get; set; }

        [StringLength(50)]
        public string? CCI { get; set; }

        public bool TieneDetraccion { get; set; }
    }
}