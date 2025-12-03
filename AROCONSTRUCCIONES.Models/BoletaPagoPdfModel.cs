using System;

namespace AROCONSTRUCCIONES.Models
{
    public class BoletaPagoPdfModel
    {
        // Datos del cálculo específico (Sueldo, descuentos, neto)
        public DetallePlanilla Detalle { get; set; }

        // Datos de la semana (Fecha inicio, fin)
        public PlanillaSemanal Cabecera { get; set; }

        // Datos personales (Nombre, DNI, Cargo)
        public Trabajador Trabajador { get; set; }

        // Ruta del logo para el encabezado
        public string LogoPath { get; set; }
    }
}