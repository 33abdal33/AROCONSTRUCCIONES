using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class ItemDespachoDto
    {
        public int MaterialId { get; set; }
        public int DetalleRequerimientoId { get; set; }
        public decimal CantidadADespachar { get; set; }
    }
}
