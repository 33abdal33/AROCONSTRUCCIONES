using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    // DTO simple para la carga de Excel (Mapeo directo de columnas)
    public class PartidaImportExcelDto
    {
        public string Item { get; set; }
        public string Descripcion { get; set; }
        public string Unidad { get; set; }
        public double Metrado { get; set; }
        public double PrecioUnitario { get; set; }
    }
}
