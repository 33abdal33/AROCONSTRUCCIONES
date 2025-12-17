using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class DetalleOrdenCompraCreateDto
    {
        public int IdMaterial { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PorcentajeDescuento { get; set; } = 0;

        // ¡CRÍTICO PARA LA TRAZABILIDAD!
        // Aquí viaja el ID del requerimiento original para descontar el stock pendiente
        public int? IdDetalleRequerimiento { get; set; }
    }
}
