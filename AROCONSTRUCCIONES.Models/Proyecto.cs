using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class Proyecto : EntityBase
    {
        public string CodigoProyecto { get; set; }
        public string NombreProyecto { get; set; }

        public string? NombreCliente { get; set; }
        public string? Ubicacion { get; set; }

        // --- Campos para el Dashboard (¡NUEVOS!) ---
        public string? Responsable { get; set; } // "Ing. Carlos Méndez"
        public int AvancePorcentaje { get; set; } = 0; // "78%" (El usuario lo pondrá manualmente por ahora)

        // --- Campos de Estado (¡MEJORADOS!) ---
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFinEstimada { get; set; }
        public string Estado { get; set; } = "Planificación"; // "Planificación", "En Ejecución", "Finalizado", "Pausado"

        // --- Campos de Costos (¡NUEVOS!) ---
        public decimal Presupuesto { get; set; } = 0;     // "$8,500,000"
        public decimal CostoEjecutado { get; set; } = 0;  // "$6,630,000" (Esto se llenará desde Logística)

        // --- Conexiones (Sin cambios) ---
        public ICollection<Requerimiento>? Requerimientos { get; set; }
    }
}
