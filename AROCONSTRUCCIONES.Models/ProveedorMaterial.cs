using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class ProveedorMaterial
    {
        public int ProveedorId { get; set; }
        public Proveedor Proveedor { get; set; }

        // Clave Foránea al Material
        public int MaterialId { get; set; }
        public Material Material { get; set; }
    }
}
