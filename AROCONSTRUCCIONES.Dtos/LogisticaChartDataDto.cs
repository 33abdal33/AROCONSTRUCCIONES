using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Dtos
{
    public class LogisticaChartDataDto
    {
        public List<ChartItemDto> InversionPorAlmacen { get; set; } = new();
        public List<ChartItemDto> ConsumoTopMateriales { get; set; } = new();
    }
}
