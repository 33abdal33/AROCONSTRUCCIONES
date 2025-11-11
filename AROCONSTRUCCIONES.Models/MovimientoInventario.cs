using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // Necesario para [NotMapped]
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class MovimientoInventario : EntityBase
    {
        // =========================================================
        // RELACIONES OBLIGATORIAS
        // =========================================================
        public int MaterialId { get; set; }
        public Material? Material { get; set; } // Asegúrate de tener la clase Material

        public int AlmacenId { get; set; }
        public Almacen? Almacen { get; set; } // Asegúrate de tener la clase Almacen

        // =========================================================
        // ⭐ PROPIEDAD DE RESPONSABLE (SIMPLE STRING) ⭐
        // =========================================================
        public string? Responsable { get; set; } // Guarda el nombre del responsable (Ej: "Juan Pérez")

        // =========================================================
        // DATOS DE TRANSACCIÓN
        // =========================================================
        public string TipoMovimiento { get; set; } // "INGRESO" o "SALIDA"
        public decimal Cantidad { get; set; }
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        // ⭐ CAMPO AÑADIDO: MOTIVO (Clasificación, Ej: COMPRA, CONSUMO, AJUSTE) ⭐
        public string? Motivo { get; set; }

        public string? Notas { get; set; } // Usado para el campo de Notas Adicionales/Referencia

        public decimal PrecioUnitario { get; set; } // (Costo/Precio del documento)
        public string? NroFacturaGuia { get; set; }

        public decimal CostoUnitarioMovimiento { get; set; } // Costo final usado en el Kárdex (CUPM o Costo de Compra)
        public int? ProveedorId { get; set; } // Opcional
        public Proveedor? Proveedor { get; set; } // Asegúrate de tener la clase Proveedor

        // =========================================================
        // ⭐ CAMPO AÑADIDO: STOCK FINAL (CRÍTICO) ⭐
        // =========================================================
        public decimal StockFinal { get; set; } // El saldo de inventario después de este movimiento

        // Propiedad calculada
        [NotMapped]
        public decimal TotalCosto => Cantidad * CostoUnitarioMovimiento;
    }
}
