using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    // Heredamos de IdentityUser para obtener todos los campos
    // de Identity (Email, PasswordHash, PhoneNumber, etc.)
    public class ApplicationUser : IdentityUser
    {
        // Aquí puedes añadir los campos que quieras
        [StringLength(100)]
        public string? NombreCompleto { get; set; }

        // Puedes añadir más: Cargo, DNI, etc.
    }
}
