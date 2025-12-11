using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AROCONSTRUCCIONES.Dtos
{
    public class MovimientoInventarioDto
    {
        // =========================================================
        // ⭐ CAMPOS DE FORMULARIO (INPUT) ⭐
        // =========================================================

        // Usado para identificar el movimiento si se necesita (ej. edición)
        public int Id { get; set; }
        // --- ¡AÑADIR ESTE CAMPO! ---
        [DisplayName("Proyecto")]
        public int? IdProyecto { get; set; } // Nullable, por si es una salida que no es de proyecto

        [DisplayName("Partida Presupuestaria")]
        public int? PartidaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el material.")]
        [DisplayName("Material")]
        public int MaterialId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el almacén de destino.")]
        [DisplayName("Almacén Destino")]
        public int AlmacenId { get; set; }

        [Required(ErrorMessage = "Debe ingresar una cantidad.")]
        [Range(0.01, 99999999.99, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public decimal Cantidad { get; set; }

        // Campo de costo (solo relevante y requerido para INGRESOS)
        [Required(ErrorMessage = "Debe especificar el costo unitario de compra.")]
        [DisplayName("Costo Unitario (S/.)")]
        [Range(0.00, 99999999.99, ErrorMessage = "El costo no puede ser negativo.")] // Permitir 0.00 si se decide usar para SALIDAS
        public decimal CostoUnitarioCompra { get; set; }

        [Required(ErrorMessage = "Debe ingresar el número de factura/guía.")]
        [DisplayName("Nro. Factura/Guía")]
        [StringLength(50, ErrorMessage = "La referencia no puede exceder los 50 caracteres.")]
        public string NroFacturaGuia { get; set; } = string.Empty;

        [DisplayName("Proveedor")]
        public int? ProveedorId { get; set; }

        // Asumiendo que guardas el ID del usuario que registra
        public int? ResponsableId { get; set; }

        // ⭐ CAMPO AÑADIDO: MOTIVO (Para el formulario y la tabla de lectura) ⭐
        [Required(ErrorMessage = "Debe seleccionar un motivo para el movimiento.")]
        [DisplayName("Motivo del Movimiento")]
        public string Motivo { get; set; } = string.Empty;

        [DisplayName("Notas Adicionales")]
        [StringLength(255, ErrorMessage = "Las notas no pueden exceder los 255 caracteres.")]
        public string? Notas { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayName("Fecha de Ingreso")]
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        public decimal Total => Cantidad * CostoUnitarioCompra;
        // 1. Tipo y Costo Real
        public string TipoMovimiento { get; set; } = string.Empty; // "INGRESO" o "SALIDA"
        public decimal CostoUnitarioMovimiento { get; set; } // Costo Real aplicado (CUPM en Salida, Compra en Ingreso)

        // 2. Datos del Material (Relacionado)
        public string MaterialCodigo { get; set; } = string.Empty;
        public string MaterialNombre { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty; // Unidad está en la tabla Material

        // 3. Datos del Almacén (Relacionado)
        public string AlmacenNombre { get; set; } = string.Empty;

        // 4. Datos del Responsable (Relacionado)
        // Se mapea desde la relación con la tabla de Usuarios/Empleados
        public string ResponsableNombre { get; set; } = string.Empty;

        // 5. Stock y Saldo (Calculado o Almacenado en la Entidad MovimientoInventario)
        public decimal StockFinal { get; set; }

    }
}
