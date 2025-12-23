using AROCONSTRUCCIONES.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IExportService
    {
        byte[] GenerarExcelStock(IEnumerable<InventarioDto> datos);
        byte[] GenerarExcelConsumo(IEnumerable<ConsumoMaterialProyectoDto> datos, string nombreProyecto);
    }
}
