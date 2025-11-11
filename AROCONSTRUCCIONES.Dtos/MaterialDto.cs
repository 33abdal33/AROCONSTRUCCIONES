using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class MaterialDto
    {
        public int Id { get; set; } 
        public string Codigo { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        public string? UnidadMedida { get; set; }

        public string? Categoria { get; set; }

        public decimal StockActual { get; set; } = 0;

        public decimal StockMinimo { get; set; } = 0;

        public decimal PrecioUnitario { get; set; } = 0;
        // ⭐⭐ CAMPO AÑADIDO: Para mostrar el Costo de Inventario ⭐⭐
        public decimal CostoUnitarioPromedio { get; set; } = 0;

        public bool Estado { get; set; } = true;
    }
}
