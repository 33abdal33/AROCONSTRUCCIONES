using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class PartidaDto
    {
        public int Id { get; set; }
        public int ProyectoId { get; set; }

        public string Item { get; set; } // 01.01.01
        public string Descripcion { get; set; }
        public string? Unidad { get; set; }

        public decimal Metrado { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Parcial { get; set; } // Metrado * Precio

        public decimal CostoEjecutado { get; set; } // Lo real gastado (se llenará luego)

        public bool EsTitulo { get; set; }
    }
}
