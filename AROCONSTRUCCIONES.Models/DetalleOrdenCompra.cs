using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // Necesario para ForeignKey
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

        // --- MEJORA 1: TRAZABILIDAD (CRÍTICO) ---
        // Esto conecta la compra con el pedido original.
        // Es nullable (int?) porque podrías hacer una compra directa sin requerimiento (urgencias).
        public int? IdDetalleRequerimiento { get; set; }

        [ForeignKey("IdDetalleRequerimiento")]
        public DetalleRequerimiento? DetalleRequerimientoOrigen { get; set; }

        // --- DATOS ECONÓMICOS ---
        [Column(TypeName = "decimal(18,4)")] // Recomendado para precisión en cantidades
        public decimal Cantidad { get; set; }

        [Column(TypeName = "decimal(18,2)")] // Moneda estándar
        public decimal PrecioUnitario { get; set; } // Costo unitario sin IGV

        // MEJORA 2: Manejo de Descuentos por ítem
        // A veces el proveedor te dice: "Este taladro cuesta 100, pero te doy 10% de descuento".
        public decimal PorcentajeDescuento { get; set; } = 0;

        // MEJORA 3: Persistencia del Subtotal
        // Lo guardamos en BD para facilitar reportes SQL/PowerBI sin recalcular
        [Column(TypeName = "decimal(18,2)")]
        public decimal ImporteTotal { get; set; }

        // --- CONTROL DE ALMACÉN ---
        // Cantidad que ya llegó a obra/almacén.
        // Si Cantidad == CantidadRecibida, esta línea está cerrada.
        [Column(TypeName = "decimal(18,4)")]
        public decimal CantidadRecibida { get; set; } = 0;

        // Método auxiliar para calcular el total antes de guardar
        public void CalcularTotal()
        {
            decimal montoBruto = Cantidad * PrecioUnitario;
            decimal montoDescuento = montoBruto * (PorcentajeDescuento / 100);
            ImporteTotal = montoBruto - montoDescuento;
        }
    }
}