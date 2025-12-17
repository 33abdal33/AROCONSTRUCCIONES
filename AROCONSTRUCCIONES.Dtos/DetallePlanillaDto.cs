using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class DetallePlanillaDto
    {
        public int Id { get; set; } // <--- ESTO ES NECESARIO PARA EL LINK DE DESCARGA
        public int TrabajadorId { get; set; }
        public string TrabajadorNombre { get; set; }
        public string Cargo { get; set; }
        public string SistemaPension { get; set; }

        // Asistencia
        public int DiasTrabajados { get; set; }
        public decimal HorasNormales { get; set; }
        public decimal HorasExtras60 { get; set; }
        public decimal HorasExtras100 { get; set; }

        // Ingresos
        public decimal JornalBasico { get; set; }
        public decimal SueldoBasico { get; set; } // H.N * (Jornal/8)
        public decimal PagoExtras { get; set; }
        public decimal BUC { get; set; } // Bonificación Unificada (30-32%)
        public decimal Indemnizacion { get; set; } // Nuevo
        public decimal Vacaciones { get; set; }    // Nuevo
        public decimal Movilidad { get; set; } // Pasajes
        public decimal TotalBruto { get; set; }

        public decimal Gratificacion { get; set; }
        public decimal BonificacionExtraordinaria { get; set; }
        public decimal AporteEsSalud { get; set; }

        // Descuentos
        public decimal AportePension { get; set; } // AFP/ONP
        public decimal Conafovicer { get; set; }
        public decimal TotalDescuentos { get; set; }

        // Neto
        public decimal NetoAPagar { get; set; }
    }
}
