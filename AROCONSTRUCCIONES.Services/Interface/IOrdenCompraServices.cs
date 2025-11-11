using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Interface
{
    public interface IOrdenCompraServices
    {
        Task<IEnumerable<OrdenCompraListDto>> GetAllOrdenesCompraAsync();
        Task<OrdenCompra> CreateOrdenCompraAsync(OrdenCompraCreateDto dto);
    }
}
