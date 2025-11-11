using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class Inventario 
    {
        // Claves Foráneas
        public int MaterialId { get; set; }
        public int AlmacenId { get; set; }

        // Propiedades de Valor
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal CostoPromedio { get; set; }
        // ⭐ CAMPO ADICIONAL PARA LA GESTIÓN DEL ESTADO/ALERTA ⭐
        // 0 = Normal, 1 = Bajo, 2 = Crítico. Esto ayuda a filtrar en DB.
        public int NivelAlerta { get; set; }
        public DateTime FechaUltimoMovimiento { get; set; }

        // ⭐⭐ PROPIEDADES DE NAVEGACIÓN AÑADIDAS ⭐⭐

        // 1. Relación con Material: 
        // Permite a EF Core saber a qué material apunta MaterialId
        public Material Material { get; set; }

        // 2. Relación con Almacén: 
        // Permite a EF Core saber a qué almacén apunta AlmacenId
        public Almacen Almacen { get; set; }

        // ⭐ PROPIEDAD CALCULADA (NO SE GUARDA EN DB) ⭐
        // Representa el "Valor Total" de la tabla: Stock * CostoPromedio
        // Se ignora en la configuración de EF Core.
        public decimal ValorTotal => StockActual * CostoPromedio;
    }
}
