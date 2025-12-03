using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class CuentaBancaria : EntityBase
    {
        [Required]
        [StringLength(50)]
        public string BancoNombre { get; set; } // Ej: BCP, BBVA, CAJA CHICA OBRA A

        [Required]
        [StringLength(50)]
        public string NumeroCuenta { get; set; }

        [StringLength(50)]
        public string? CCI { get; set; }

        [Required]
        [StringLength(10)]
        public string Moneda { get; set; } = "PEN"; // PEN, USD

        // Saldo contable
        public decimal SaldoInicial { get; set; } = 0;
        public decimal SaldoActual { get; set; } = 0;

        [StringLength(100)]
        public string? Titular { get; set; } // Nombre de la empresa o del responsable (si es caja chica)

        public string? Descripcion { get; set; } // Ej: "Cuenta Corriente Soles Principal"

        public bool Activo { get; set; } = true;
    }
}
