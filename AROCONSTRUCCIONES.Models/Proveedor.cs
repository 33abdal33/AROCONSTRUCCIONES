using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Models
{
    public class Proveedor : EntityBase
    {
        public string? RazonSocial { get; set; }
        public string? RUC { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Especialidad { get; set; } // Ej: "Agregados", "Ferretería", "Eléctricos"
        public string? NombreContacto { get; set; } // Ej: "Carlos Hannco"

        public bool Estado { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public ICollection<OrdenCompra>? OrdenesCompra { get; set; }
        // Conexión a la tabla "Muchos-a-Muchos"
        public ICollection<ProveedorMaterial> ProveedorMateriales { get; set; }


    }
}
