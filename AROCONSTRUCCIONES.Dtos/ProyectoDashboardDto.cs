using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class ProyectoDashboardDto
    {
        public int ProyectosActivos { get; set; }
        public int AvancePromedio { get; set; } // 0-100
        public int EnCronograma { get; set; } // Conteo de proyectos
        public decimal PresupuestoTotal { get; set; }
        public decimal EjecutadoTotal { get; set; }
    }
}
