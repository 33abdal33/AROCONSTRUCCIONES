using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AROCONSTRUCCIONES.Dtos
{
    public class MovimientoInventarioDto
    {
        public int Id { get; set; }

        [DisplayName("Proyecto")]
        public int? ProyectoId { get; set; }

        [DisplayName("Partida Presupuestaria")]
        public int? PartidaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el material.")]
        [DisplayName("Material")]
        public int MaterialId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el almacén.")]
        [DisplayName("Almacén")]
        public int AlmacenId { get; set; }

        // --- CAMPOS PARA TRAZABILIDAD (NUEVOS) ---

        [DisplayName("ID Detalle Requerimiento")]
        public int? DetalleRequerimientoId { get; set; } // Para Salidas (Atención de Pedidos)

        [DisplayName("ID Detalle Orden Compra")]
        public int? DetalleOrdenCompraId { get; set; } // Para Ingresos (Recepciones)

        // -----------------------------------------

        [Required(ErrorMessage = "Debe ingresar una cantidad.")]
        [Range(0.01, 99999999.99, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public decimal Cantidad { get; set; }

        [Required(ErrorMessage = "Debe especificar el costo unitario.")]
        [DisplayName("Costo Unitario (S/.)")]
        [Range(0.00, 99999999.99, ErrorMessage = "El costo no puede ser negativo.")]
        public decimal CostoUnitarioCompra { get; set; }

        [Required(ErrorMessage = "Debe ingresar el número de factura/guía.")]
        [DisplayName("Nro. Factura/Guía")]
        [StringLength(50, ErrorMessage = "La referencia no puede exceder los 50 caracteres.")]
        public string NroFacturaGuia { get; set; } = string.Empty;

        [DisplayName("Proveedor")]
        public int? ProveedorId { get; set; }

        public int? ResponsableId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un motivo para el movimiento.")]
        [DisplayName("Motivo del Movimiento")]
        public string Motivo { get; set; } = string.Empty;

        [DisplayName("Notas Adicionales")]
        [StringLength(255, ErrorMessage = "Las notas no pueden exceder los 255 caracteres.")]
        public string? Notas { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Fecha")]
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        // Propiedades calculadas y de visualización
        public decimal Total => Cantidad * CostoUnitarioCompra;
        public string TipoMovimiento { get; set; } = string.Empty; // INGRESO / SALIDA
        public decimal CostoUnitarioMovimiento { get; set; }
        public string MaterialCodigo { get; set; } = string.Empty;
        public string MaterialNombre { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public string AlmacenNombre { get; set; } = string.Empty;
        public string ResponsableNombre { get; set; } = string.Empty;
        public decimal StockFinal { get; set; }
    }
}