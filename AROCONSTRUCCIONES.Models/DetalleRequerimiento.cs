using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class DetalleRequerimiento : EntityBase
    {

        public int IdRequerimiento { get; set; }
        public Requerimiento? Requerimiento { get; set; }

        public int IdMaterial { get; set; }
        public Material? Material { get; set; }

        public decimal CantidadSolicitada { get; set; }
        public decimal CantidadAtendida { get; set; } = 0;

    }
}
