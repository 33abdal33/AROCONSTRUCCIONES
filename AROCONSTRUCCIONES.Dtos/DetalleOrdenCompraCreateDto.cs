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

        // --- NUEVO: Para mostrar el nombre en el formulario ---
        public string? MaterialNombre { get; set; }
        // ------------------------------------------------------

        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PorcentajeDescuento { get; set; } = 0;
        public int? IdDetalleRequerimiento { get; set; }
    }
}
