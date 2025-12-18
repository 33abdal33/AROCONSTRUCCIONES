using AROCONSTRUCCIONES.Models;

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
