using AROCONSTRUCCIONES.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Aquí expones todas tus interfaces de repositorio
        IAlmacenRepository Almacenes { get; }
        IMaterialRepository Materiales { get; }
        IProveedorRepository Proveedores { get; }
        IOrdenCompraRepository OrdenesCompra { get; }
        IInventarioRepository Inventario { get; }
        IMovimientoInventarioRepository MovimientosInventario { get; }
        IProyectoRepository Proyectos { get; }
        IRequerimientoRepository Requerimientos { get; }
        // ... y cualquier otro repositorio que tengas ...
        ApplicationDbContext Context { get; } // Para acceder a tablas sin repo (ej. ProveedorMateriales)

        // Este es el método clave que guardará todos los cambios
        Task<int> SaveChangesAsync();
    }
}
