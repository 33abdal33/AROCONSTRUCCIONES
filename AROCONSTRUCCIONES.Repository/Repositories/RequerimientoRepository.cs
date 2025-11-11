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
    public class RequerimientoRepository : RepositoryBase<Requerimiento>, IRequerimientoRepository
    {
        private readonly ApplicationDbContext _context;

        public RequerimientoRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Requerimiento>> GetRequerimientosPorProyectoAsync(int proyectoId)
        {
            return await _context.Requerimientos
                .Where(r => r.IdProyecto == proyectoId)
                .OrderByDescending(r => r.Fecha)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Requerimiento> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Requerimientos
                .Include(r => r.Proyecto)
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Material)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
