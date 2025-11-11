using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Repository.Interfaces
{
    public interface IRepositoryBaseReadOnly<TEntity> where TEntity : class
    {
        // Los métodos que necesitas para Inventario
        Task<IEnumerable<TEntity>> GetAllAsync();
        // Task<ICollection<TEntity>> FindAsync... (Si lo necesitas)
    }
}
