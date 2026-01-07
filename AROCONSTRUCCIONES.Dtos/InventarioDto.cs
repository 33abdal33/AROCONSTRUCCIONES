using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AROCONSTRUCCIONES.Dtos
{
    public class InventarioDto
    {
        [Required]
        public int MaterialId { get; set; }

        [Required]
        public int AlmacenId { get; set; }

        // --- PROPIEDAD NUEVA PARA EL FILTRO ---
        public int? ProyectoId { get; set; }

        [DisplayName("Código")]
        public string MaterialCodigo { get; set; }

        [DisplayName("Material")]
        public string MaterialNombre { get; set; }

        [DisplayName("Categoría")]
        public string MaterialCategoria { get; set; }

        [DisplayName("Unidad")]
        public string MaterialUnidadMedida { get; set; }

        [DisplayName("PrecioUnitario")]
        public decimal MaterialPrecioUnidad { get; set; }

        // --- PROPIEDAD NUEVA PARA LA VISTA ---
        [DisplayName("Almacén")]
        public string AlmacenNombre { get; set; }

        [DisplayName("Ubicación")]
        public string AlmacenUbicacion { get; set; }

        [DisplayName("Stock")]
        public decimal StockActual { get; set; }

        [DisplayName("Stock Mín.")]
        public decimal StockMinimo { get; set; }

        [DisplayName("Precio Prom.")]
        public decimal CostoPromedio { get; set; }

        [DisplayName("Estado")]
        public int NivelAlerta { get; set; }

        [DisplayName("Fecha Última Entrada")]
        public DateTime FechaUltimoMovimiento { get; set; }

        [DisplayName("Valor Total")]
        public decimal ValorTotal => StockActual * CostoPromedio;

        public string EstadoTexto => NivelAlerta == 0 ? "Normal" :
                                     NivelAlerta == 1 ? "Bajo" : "Crítico";
    }
}