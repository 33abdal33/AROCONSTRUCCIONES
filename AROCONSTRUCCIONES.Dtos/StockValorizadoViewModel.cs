using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class StockValorizadoViewModel
    {
        public decimal TotalValorizadoGeneral { get; set; }
        public int TotalItems { get; set; }
        public string AlmacenFiltro { get; set; } = "Todos";

        // La lista de inventario detallada
        public IEnumerable<InventarioDto> Items { get; set; }
    }
}
