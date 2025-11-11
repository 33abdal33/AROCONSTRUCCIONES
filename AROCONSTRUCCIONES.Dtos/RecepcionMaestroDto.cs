using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class RecepcionMaestroDto
    {
        public int OrdenCompraId { get; set; }
        public string? CodigoOC { get; set; }
        public string? ProveedorNombre { get; set; }
        public int ProveedorId { get; set; } // Lo necesitamos para el Movimiento

        // --- Campos de Entrada ---
        [Required]
        [Display(Name = "Almacén de Destino")]
        public int AlmacenDestinoId { get; set; }

        [Required]
        [Display(Name = "Documento (Factura/Guía)")]
        public string? NroFacturaGuia { get; set; }

        [Required]
        [Display(Name = "Responsable Recepción")]
        public string ResponsableNombre { get; set; }

        public DateTime? FechaRecepcion { get; set; } = DateTime.Now;

        // La lista de materiales
        public List<RecepcionDetalleDto> Detalles { get; set; } = new();
    }
}
