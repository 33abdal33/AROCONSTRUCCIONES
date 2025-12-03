using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class Trabajador : EntityBase
    {
        [Required]
        [StringLength(15)] // DNI o Carnet Extranjería
        public string NumeroDocumento { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombres { get; set; }

        [Required]
        [StringLength(100)]
        public string Apellidos { get; set; }

        // Propiedad de solo lectura para mostrar nombre completo
        public string NombreCompleto => $"{Apellidos}, {Nombres}";

        [StringLength(20)]
        public string? Telefono { get; set; }

        // --- Datos Laborales ---

        public int CargoId { get; set; }
        [ForeignKey("CargoId")]
        public Cargo Cargo { get; set; }

        // Sistema de Pensiones (AFP/ONP) - Clave para la planilla
        [StringLength(20)]
        public string SistemaPension { get; set; } = "ONP"; // ONP, INTEGRA, PRIMA, HABITAT, PROFUTURO

        [StringLength(20)]
        public string? Cuspp { get; set; } // Código de afiliado AFP

        [StringLength(50)]
        public string? NumeroCuenta { get; set; }

        [StringLength(50)]
        public string? Banco { get; set; }

        public DateTime FechaIngreso { get; set; } = DateTime.Now;
        public DateTime? FechaCese { get; set; }

        public bool Estado { get; set; } = true; // Activo / Cesado

        // Para saber en qué obra está asignado actualmente por defecto
        public int? ProyectoActualId { get; set; }
        [ForeignKey("ProyectoActualId")]
        public Proyecto? ProyectoActual { get; set; }
    }
}
