using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class RecepcionDetalleDto
    {
        public int DetalleOrdenCompraId { get; set; }
        public int MaterialId { get; set; }
        public string? MaterialNombre { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;

        public decimal CantidadPedida { get; set; }
        public decimal CantidadYaRecibida { get; set; }

        public decimal CantidadPendiente => CantidadPedida - CantidadYaRecibida;

        // El único campo que el usuario llenará
        [Display(Name = "Cantidad a Recibir")]
        [Range(0, double.MaxValue)]
        public decimal CantidadARecibir { get; set; } = 0;
    }
}
