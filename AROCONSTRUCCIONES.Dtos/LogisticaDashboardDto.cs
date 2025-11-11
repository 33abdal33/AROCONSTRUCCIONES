using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class LogisticaDashboardDto
    {
        public decimal ValorInventario { get; set; }
        public int OrdenesPendientes { get; set; }
        public int MovimientosMes { get; set; }
        public int AlertasStock { get; set; }
    }
}
