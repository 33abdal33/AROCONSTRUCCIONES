using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Repository.Interfaces;

namespace AROCONSTRUCCIONES.Repository.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public ApplicationDbContext Context => _context;
        public IAlmacenRepository Almacenes { get; private set; }
        public IMaterialRepository Materiales { get; private set; }
        public IProveedorRepository Proveedores { get; private set; }
        public IOrdenCompraRepository OrdenesCompra { get; private set; }

        // --- IMPLEMENTACIÓN ---
        public IDetalleOrdenCompraRepository DetalleOrdenCompra { get; private set; }
        // ----------------------

        public IInventarioRepository Inventario { get; private set; }
        public IMovimientoInventarioRepository MovimientosInventario { get; private set; }
        public IProyectoRepository Proyectos { get; private set; }
        public IRequerimientoRepository Requerimientos { get; private set; }

        // --- IMPLEMENTACIÓN (SOLUCIONA EL ERROR) ---
        public IDetalleRequerimientoRepository DetalleRequerimiento { get; private set; }
        // -------------------------------------------

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            Almacenes = new AlmacenRepository(_context);
            Materiales = new MaterialRepository(_context);
            Proveedores = new ProveedorRepository(_context);
            OrdenesCompra = new OrdenCompraRepository(_context);

            // --- INICIALIZAR ---
            DetalleOrdenCompra = new DetalleOrdenCompraRepository(_context);
            // -------------------

            Inventario = new InventarioRepository(_context);
            MovimientosInventario = new MovimientoInventarioRepository(_context);
            Proyectos = new ProyectoRepository(_context);
            Requerimientos = new RequerimientoRepository(_context);

            // --- INICIALIZAR (CRÍTICO) ---
            DetalleRequerimiento = new DetalleRequerimientoRepository(_context);
            // -----------------------------
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}