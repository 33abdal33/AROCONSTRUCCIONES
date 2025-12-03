using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class CuentaBancariaDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del banco es obligatorio")]
        [Display(Name = "Banco / Caja")]
        public string BancoNombre { get; set; }

        [Required(ErrorMessage = "El número de cuenta es obligatorio")]
        [Display(Name = "Nro. Cuenta")]
        public string NumeroCuenta { get; set; }

        public string? CCI { get; set; }

        [Required]
        public string Moneda { get; set; } = "PEN";

        [Display(Name = "Saldo Inicial")]
        public decimal SaldoInicial { get; set; }

        // El saldo actual se calcula, no se edita directamente en el formulario
        public decimal SaldoActual { get; set; }

        public string? Titular { get; set; }
        public bool Activo { get; set; } = true;
    }
}