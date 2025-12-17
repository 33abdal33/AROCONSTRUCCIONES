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

        // --- AGREGA ESTO ---
        IDetalleOrdenCompraRepository DetalleOrdenCompra { get; } // Útil para validaciones
        // -------------------

        IInventarioRepository Inventario { get; }
        IMovimientoInventarioRepository MovimientosInventario { get; }
        IProyectoRepository Proyectos { get; }
        IRequerimientoRepository Requerimientos { get; }

        // --- AGREGA ESTO (SOLUCIONA EL ERROR) ---
        IDetalleRequerimientoRepository DetalleRequerimiento { get; }
        // ----------------------------------------

        ApplicationDbContext Context { get; }
        Task<int> SaveChangesAsync();
    }
}
