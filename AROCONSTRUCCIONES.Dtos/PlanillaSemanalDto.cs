using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class PlanillaSemanalDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int ProyectoId { get; set; }
        public string ProyectoNombre { get; set; }
        public string Estado { get; set; } = "Borrador";

        public List<DetallePlanillaDto> Detalles { get; set; } = new();

        // Totales calculados
        public decimal TotalBruto => Detalles.Sum(d => d.TotalBruto);
        public decimal TotalNeto => Detalles.Sum(d => d.NetoAPagar);
        public int TotalObreros => Detalles.Count;
    }
}
