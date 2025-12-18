using AROCONSTRUCCIONES.Models;

namespace AROCONSTRUCCIONES.Repository.Interfaces
{
    public interface IInventarioRepository : IRepositoryBaseReadOnly<Inventario>
    {
        // Este método es específico y crucial para Inventario:
        // Permite buscar un saldo por la combinación de sus claves foráneas
        Task<Inventario?> FindByKeysAsync(int materialId, int almacenId);

        // Métodos para actualizar e insertar, ya que no se encuentran en IRepositoryBaseReadOnly:
        Task<Inventario> AddAsync(Inventario entity);
        Task UpdateAsync(Inventario entity);
    }
}
