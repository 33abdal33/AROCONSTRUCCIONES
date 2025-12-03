using AROCONSTRUCCIONES.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IFinanzasService
    {
        Task<IEnumerable<CuentaBancariaDto>> GetAllCuentasAsync();
        Task<CuentaBancariaDto?> GetCuentaByIdAsync(int id);
        Task CreateCuentaAsync(CuentaBancariaDto dto);
        Task UpdateCuentaAsync(int id, CuentaBancariaDto dto);
        Task<IEnumerable<MovimientoBancarioDto>> GetMovimientosAsync(int? cuentaId, DateTime? inicio, DateTime? fin);
    }
}
