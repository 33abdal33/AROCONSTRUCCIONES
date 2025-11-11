using AROCONSTRUCCIONES.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Repository.Interfaces
{
    public interface IOrdenCompraRepository : IRepositoryBase<OrdenCompra>
    {
        // Método específico para cargar la OC con todas sus relaciones
        Task<OrdenCompra?> GetByIdWithDetailsAsync(int id);

        // Método para la vista de índice (Index)
        Task<IEnumerable<OrdenCompra>> GetAllWithProveedorAsync();
    }
}
