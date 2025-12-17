using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System; // Agregado
using System.Collections.Generic; // Agregado
using System.Linq; // Agregado
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Repository.Repositories
{
    public abstract class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : EntityBase
    {
        private readonly DbContext context;

        protected RepositoryBase(DbContext context)
        {
            this.context = context;
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await context.Set<TEntity>().AddAsync(entity);
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            var item = await GetByIdAsync(id);
            if (item is not null)
            {
                context.Set<TEntity>().Remove(item);
                // Si borras fuera de transacción, considera reactivar el SaveChanges aquí o manejarlo en el servicio.
                // await context.SaveChangesAsync(); 
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
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
        }

        // Tu método Async existente (está bien mantenerlo)
        public virtual async Task UpdateAsync(TEntity entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            await Task.CompletedTask;
        }

        // --- AGREGA ESTE MÉTODO (SOLUCIÓN AL ERROR) ---
        public virtual void Update(TEntity entity)
        {
            // Opción A: Usando el método Update del DbSet (Recomendado, maneja Attach automático)
            context.Set<TEntity>().Update(entity);

            // Opción B: Forzando el estado (Como lo tenías en el Async)
            // context.Entry(entity).State = EntityState.Modified;
        }
    }
}