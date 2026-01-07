using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class Almacen :EntityBase
    {
        public string Nombre { get; set; }
        public string? Ubicacion { get; set; }
        public string? Responsable { get; set; }
        public bool Estado { get; set; } = true;

        // --- ESTA ES LA VINCULACIÓN QUE FALTA ---
        public int? ProyectoId { get; set; }
        public Proyecto? Proyecto { get; set; }

        public ICollection<MovimientoInventario>? Movimientos { get; set; }
    }
}
