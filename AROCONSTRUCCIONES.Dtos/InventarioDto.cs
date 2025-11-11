using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class InventarioDto
    {
        // 1. Claves para Referencia (Necesarias para Acciones como 'Detalles' o 'Salida')

        [Required]
        public int MaterialId { get; set; }

        [Required]
        public int AlmacenId { get; set; }


        // 2. CAMPOS DE MATERIAL (Obtenidos a través de la relación)

        [DisplayName("Código")]
        public string MaterialCodigo { get; set; }

        [DisplayName("Material")]
        public string MaterialNombre { get; set; }

        [DisplayName("Categoría")]
        public string MaterialCategoria { get; set; }

        [DisplayName("Unidad")]
        public string MaterialUnidadMedida { get; set; } // Ejemplo: Bolsa 42.5kg, m², etc.
        [DisplayName("PrecioUnitario")]
        public decimal MaterialPrecioUnidad { get; set; }


        // 3. CAMPOS DE ALMACÉN (Obtenidos a través de la relación)

        [DisplayName("Ubicación")]
        public string AlmacenUbicacion { get; set; } // Muestra "Almacén Principal - A1"

        // Puedes agregar el responsable si es necesario para la vista
        // public string AlmacenResponsable { get; set; }


        // 4. CAMPOS DE SALDO (De la entidad Inventario)

        [DisplayName("Stock")]
        public decimal StockActual { get; set; }

        [DisplayName("Stock Mín.")]
        public decimal StockMinimo { get; set; }

        [DisplayName("Precio Prom.")]
        public decimal CostoPromedio { get; set; }

        // Este campo indica el nivel de alerta (0=Normal, 1=Bajo, 2=Crítico)
        [DisplayName("Estado")]
        public int NivelAlerta { get; set; }

        [DisplayName("Fecha Última Entrada")]
        public DateTime FechaUltimoMovimiento { get; set; }


        // 5. PROPIEDAD CALCULADA PARA LA VISTA

        [DisplayName("Valor Total")]
        // El Valor Total (Stock * Costo Promedio) se calcula aquí, no se persiste.
        public decimal ValorTotal => StockActual * CostoPromedio;

        // Helper para mostrar el estado como texto (Normal, Bajo)
        public string EstadoTexto => NivelAlerta == 0 ? "Normal" :
                                     NivelAlerta == 1 ? "Bajo" : "Crítico";
    }
}
