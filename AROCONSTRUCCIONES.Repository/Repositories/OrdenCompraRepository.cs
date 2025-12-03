using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Repository.Repositories
{
    public class OrdenCompraRepository : RepositoryBase<OrdenCompra>, IOrdenCompraRepository
    {
        private readonly ApplicationDbContext _context;

        public OrdenCompraRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<OrdenCompra>> GetAllWithProveedorAsync()
        {
            return await _context.OrdenesCompra
                .Include(oc => oc.Proveedor) // Incluimos al proveedor para el DTO aplanado
                .OrderByDescending(oc => oc.FechaEmision)
                .AsNoTracking()
                .ToListAsync();
        }
        // ==========================================================
        // ↓↓↓ ¡AÑADE ESTE MÉTODO COMPLETO AQUÍ! ↓↓↓
        // ==========================================================
        public async Task<OrdenCompra?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.OrdenesCompra
                .Include(oc => oc.Proveedor)
                .Include(oc => oc.Proyecto) // <--- ¡ESTA LÍNEA ES LA QUE FALTABA!
                .Include(oc => oc.Detalles)
                    .ThenInclude(d => d.Material)
                .FirstOrDefaultAsync(oc => oc.Id == id);
        }
    }
}
