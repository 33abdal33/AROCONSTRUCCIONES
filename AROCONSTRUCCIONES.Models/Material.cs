using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class Material :EntityBase
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? UnidadMedida { get; set; }
        public string? Categoria { get; set; }
        public decimal StockActual { get; set; } = 0;
        public decimal StockMinimo { get; set; } = 0;
        public decimal PrecioUnitario { get; set; } = 0;
        public bool Estado { get; set; } = true;
        // ⭐⭐ CAMPO AÑADIDO: NECESARIO PARA EL CÁLCULO DE INVENTARIO ⭐⭐
        public decimal CostoUnitarioPromedio { get; set; } = 0;

        public ICollection<DetalleRequerimiento>? DetallesRequerimiento { get; set; }
        public ICollection<DetalleOrdenCompra>? DetallesOrdenCompra { get; set; }
        public ICollection<MovimientoInventario>? MovimientosInventario { get; set; }
        // Conexión a la tabla "Muchos-a-Muchos"
        public ICollection<ProveedorMaterial> ProveedorMateriales { get; set; }

    }
}