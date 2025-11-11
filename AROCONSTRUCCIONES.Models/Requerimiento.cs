using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class Requerimiento : EntityBase
    {
        public int IdProyecto { get; set; }      // La Clave Foránea
        public Proyecto? Proyecto { get; set; }  // La Propiedad de Navegación
        public string Codigo { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string? Solicitante { get; set; }
        public string? Area { get; set; }
        public string Estado { get; set; } = "Pendiente";

        public ICollection<DetalleRequerimiento>? Detalles { get; set; }

    }
}
