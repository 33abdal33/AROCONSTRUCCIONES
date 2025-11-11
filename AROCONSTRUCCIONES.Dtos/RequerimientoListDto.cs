using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class RequerimientoListDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public DateTime Fecha { get; set; }
        public string Solicitante { get; set; }
        public string Estado { get; set; } // Pendiente, Aprobado, Despachado, Cancelado
    }
}
