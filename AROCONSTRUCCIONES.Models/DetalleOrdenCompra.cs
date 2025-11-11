using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class DetalleOrdenCompra : EntityBase
    {
        public int IdOrdenCompra { get; set; }
        public OrdenCompra? OrdenCompra { get; set; }

        public int IdMaterial { get; set; }
        public Material? Material { get; set; }

        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal CantidadRecibida { get; set; } = 0;

        public decimal Subtotal => Cantidad * PrecioUnitario;

    }
}
