using System;

namespace AROCONSTRUCCIONES.Dtos
{
    public class MovimientoBancarioDto
    {
        public int Id { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string TipoMovimiento { get; set; } // "INGRESO", "EGRESO"
        public decimal Monto { get; set; }
        public decimal SaldoDespues { get; set; }
        public string Descripcion { get; set; }
        public string NumeroOperacion { get; set; }

        // Datos de la cuenta
        public string BancoNombre { get; set; }
        public string NumeroCuenta { get; set; }
        public string Moneda { get; set; }
    }
}