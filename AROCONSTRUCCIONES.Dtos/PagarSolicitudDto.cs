using System;
using System.ComponentModel.DataAnnotations;

namespace AROCONSTRUCCIONES.Dtos
{
    public class PagarSolicitudDto
    {
        [Required(ErrorMessage = "Seleccione la cuenta de origen")]
        public int CuentaBancariaId { get; set; }
        [Required]
        public int SolicitudId { get; set; }

        [Required(ErrorMessage = "La fecha de pago es obligatoria")]
        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El número de operación es obligatorio")]
        public string NumeroOperacion { get; set; } // Voucher o Nro Transferencia

        public string? Comentario { get; set; }
    }
}