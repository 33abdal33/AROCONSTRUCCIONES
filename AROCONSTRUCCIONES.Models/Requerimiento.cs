using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class Requerimiento : EntityBase
    {
        [ForeignKey("Proyecto")]
        public int IdProyecto { get; set; }     // La Clave Foránea
        public Proyecto? Proyecto { get; set; }  // La Propiedad de Navegación
        public string Codigo { get; set; }
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        // MEJORA 1: Prioridad (Alta, Media, Baja) para que Logística sepa qué comprar primero
        public string Prioridad { get; set; } = "Media";
        // MEJORA 2: Fecha en la que se necesita el material en obra (diferente a la fecha de solicitud)
        public DateTime FechaRequerida { get; set; }
        public string? Solicitante { get; set; }
        public string? Area { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string? UsuarioAprobador { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public ICollection<DetalleRequerimiento>? Detalles { get; set; }
        public ICollection<OrdenCompra>? OrdenesCompra { get; set; } // <--- AGREGAR ESTO
    }
}
