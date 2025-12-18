using AROCONSTRUCCIONES.Models;
using System.Linq.Expressions;

namespace AROCONSTRUCCIONES.Repository.Interfaces
{
    public interface IRepositoryBase<TEntity> where TEntity : EntityBase
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<ICollection<TEntity>> FindAsync<TKey>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> orderBy);
        Task<TEntity?> GetByIdAsync(int id);
        Task<TEntity> AddAsync(TEntity entity);

        Task UpdateAsync(TEntity entity); // Tu método existente

        // --- AGREGA ESTA LÍNEA ---
        void Update(TEntity entity);      // El método que busca tu servicio
        // -------------------------

        Task DeleteAsync(int id);
    }
}