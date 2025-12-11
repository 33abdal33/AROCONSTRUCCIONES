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
        public int? ProyectoId { get; set; } // (Esto ya lo tenías implícito o explícito, asegúrate de tenerlo
        [ForeignKey("ProyectoId")]
        public Proyecto? Proyecto { get; set; }

        // --- NUEVO: Relación con Partida ---
        public int? PartidaId { get; set; }
        [ForeignKey("PartidaId")]
        public Partida? Partida { get; set; }
        public int MaterialId { get; set; }
        public Material? Material { get; set; }
        public int AlmacenId { get; set; }
        public Almacen? Almacen { get; set; }
        public string? Responsable { get; set; }
        public string TipoMovimiento { get; set; }
        public decimal Cantidad { get; set; }
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;
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
