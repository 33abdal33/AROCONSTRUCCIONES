using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Repository.Repositories
{
    // Ahora, RepositoryBase SOLO ENCOLA operaciones
    public abstract class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : EntityBase
    {
        private readonly DbContext context;

        protected RepositoryBase(DbContext context)
        {
            this.context = context;
        }

        // 1. CORREGIDO: SE ELIMINA SaveChangesAsync. Solo encola la adición.
        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await context.Set<TEntity>().AddAsync(entity);
            // 🚨 ELIMINADO: await context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var item = await GetByIdAsync(id);
            if (item is not null)
            {
                context.Set<TEntity>().Remove(item);
                // 💡 NOTA: Si este método se usa fuera de una transacción, se necesita SaveChangesAsync.
                // Como este método no forma parte del proceso actual, lo dejaremos.
                await context.SaveChangesAsync();
            }
        }

        public virtual async Task<ICollection<TEntity>> FindAsync<TKey>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TKey>> orderBy)
        {
            return await context.Set<TEntity>()
                .Where(predicate)
                .OrderBy(orderBy)
                .AsNoTracking()
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await context.Set<TEntity>()
                .AsNoTracking()
                .ToListAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            return await context.Set<TEntity>()
            .AsNoTracking() // Recomendado para lecturas
            .FirstOrDefaultAsync(e => e.Id == id); // Mejor si quieres incluir navegación
        }

        // 2. CORREGIDO: SE ELIMINA SaveChangesAsync. Solo encola la actualización.
        public virtual async Task UpdateAsync(TEntity entity)
        {
            // context.Set<TEntity>().Update(entity); <-- Esta línea es peligrosa
            // Mejor manera de garantizar que se marque como Modified si no está trackeado:
            context.Entry(entity).State = EntityState.Modified;

            // 🚨 ELIMINADO: await context.SaveChangesAsync();
            await Task.CompletedTask; // Para mantener la firma asíncrona
        }
    }
}
