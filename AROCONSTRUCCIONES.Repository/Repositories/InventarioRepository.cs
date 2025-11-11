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
    // ⭐ NO HEREDA DE RepositoryBase<Inventario> para evitar el error CS0311
    public class InventarioRepository : IInventarioRepository
    {
        private readonly ApplicationDbContext _context;

        public InventarioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // =======================================================
        // MÉTODOS HEREDADOS (IRepositoryBaseReadOnly<Inventario>)
        // =======================================================

        public async Task<IEnumerable<Inventario>> GetAllAsync()
        {
            // Nota: Aquí se deben incluir las propiedades de navegación para el DTO
            return await _context.Inventario
                                 .Include(i => i.Material)
                                 .Include(i => i.Almacen)
                                 .ToListAsync();
        }

        // =======================================================
        // MÉTODOS ESPECÍFICOS (IInventarioRepository)
        // =======================================================

        // 1. Método Crucial para la Clave Compuesta
        public async Task<Inventario?> FindByKeysAsync(int materialId, int almacenId)
        {
            return await _context.Inventario
                                 .Include(i => i.Material)
                                 .Include(i => i.Almacen)
                                 .FirstOrDefaultAsync(i => i.MaterialId == materialId && i.AlmacenId == almacenId);
        }

        // 2. Método para Agregar Nuevo Saldo
        public async Task<Inventario> AddAsync(Inventario entity)
        {
            await _context.Inventario.AddAsync(entity);
            // No se llama a SaveChanges aquí, el servicio lo hará si todo es exitoso (atomicidad)
            return entity;
        }

        // 3. Método para Actualizar Saldo Existente
        public Task UpdateAsync(Inventario entity)
        {
            // EF Core rastrea la entidad, solo la marcamos como modificada si no estamos en un contexto de cambio
            _context.Entry(entity).State = EntityState.Modified;
            // No se llama a SaveChanges aquí
            return Task.CompletedTask;
        }
    }
}
