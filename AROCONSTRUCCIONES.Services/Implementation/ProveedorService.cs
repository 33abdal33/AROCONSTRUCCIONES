using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq; // <-- ¡Asegúrate de tener este using!
using System.Threading.Tasks;
using System; // <-- Para ApplicationException

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class ProveedorService : IProveedorService
    {
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IMaterialRepository _materialRepository; // Para leer materiales
        private readonly IMaterialServices _materialService; // Para leer categorías
        private readonly ApplicationDbContext _dbContext; // Para transacciones y tablas de unión
        private readonly IMapper _mapper;

        public ProveedorService(
          IProveedorRepository proveedorRepository,
          IMaterialRepository materialRepository,
                IMaterialServices materialService,
          ApplicationDbContext dbContext,
          IMapper mapper)
        {
            _proveedorRepository = proveedorRepository;
            _materialRepository = materialRepository;
            _materialService = materialService;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        // --- Métodos CRUD simples (sin cambios) ---
        public async Task<IEnumerable<ProveedorDto>> GetAllAsync()
        {
            var proveedores = await _proveedorRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProveedorDto>>(proveedores);
        }

        public async Task<IEnumerable<ProveedorDto>> GetAllActiveAsync()
        {
            var activos = await _proveedorRepository.FindAsync(p => p.Estado == true, p => p.RazonSocial);
            return _mapper.Map<IEnumerable<ProveedorDto>>(activos);
        }

        public async Task<ProveedorDto?> GetByIdAsync(int id)
        {
            var proveedorEntity = await _proveedorRepository.GetByIdAsync(id);
            if (proveedorEntity is null) return null;
            return _mapper.Map<ProveedorDto>(proveedorEntity);
        }

        public async Task CreateAsync(ProveedorDto dto)
        {
            var entity = _mapper.Map<Proveedor>(dto);
            entity.Estado = true;
            await _proveedorRepository.AddAsync(entity);
        }

        public async Task<Proveedor> UpdateAsync(int id, ProveedorDto dto)
        {
            var existing = await _proveedorRepository.GetByIdAsync(id);
            if (existing is null) return null;
            _mapper.Map(dto, existing);
            await _proveedorRepository.UpdateAsync(existing);
            return existing;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var existing = await _proveedorRepository.GetByIdAsync(id);
            if (existing is null) return false;
            existing.Estado = false;
            await _proveedorRepository.UpdateAsync(existing);
            return true;
        }

        // --- ¡MÉTODOS NUEVOS IMPLEMENTADOS! ---

        public async Task<ProveedorEdicionViewModelDto> GetEdicionProveedorAsync(int id)
        {
            var vm = new ProveedorEdicionViewModelDto();

            // 1. Cargar las listas de soporte (Categorías y TODOS los materiales)
            vm.CategoriasMateriales = await _materialService.GetMaterialCategoriesAsync();
            var todosLosMateriales = await _materialRepository.GetAllAsync();
            vm.TodosLosMateriales = _mapper.Map<List<MaterialDto>>(todosLosMateriales);

            if (id == 0) // Es "Crear Nuevo"
            {
                // vm ya tiene un ProveedorDto vacío y listas vacías, listo para usarse
                return vm;
            }
            else // Es "Editar"
            {
                // 2. Cargar los datos del Proveedor
                vm.Proveedor = await GetByIdAsync(id);
                if (vm.Proveedor == null)
                    throw new ApplicationException("Proveedor no encontrado");

                // 3. Cargar los IDs de los materiales que YA tiene asignados
                vm.MaterialesAsignadosIds = await _dbContext.ProveedorMateriales
                    .Where(pm => pm.ProveedorId == id)
                    .Select(pm => pm.MaterialId)
                    .ToListAsync();

                return vm;
            }
        }

        public async Task UpdateProveedorCompletoAsync(ProveedorEdicionViewModelDto vm)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Guardar los datos del Proveedor (Pestaña 1)
                var proveedor = await _proveedorRepository.GetByIdAsync(vm.Proveedor.Id);
                if (proveedor == null)
                    throw new ApplicationException("Proveedor no encontrado.");

                _mapper.Map(vm.Proveedor, proveedor);
                await _proveedorRepository.UpdateAsync(proveedor);

                // (Guardamos una vez para asegurar que el proveedor existe)
                await _dbContext.SaveChangesAsync();

                // 2. Guardar los Materiales Asignados (Pestaña 2)
                var asignacionesViejas = _dbContext.ProveedorMateriales
          .Where(pm => pm.ProveedorId == vm.Proveedor.Id);

                _dbContext.ProveedorMateriales.RemoveRange(asignacionesViejas);

                // Si el usuario no seleccionó nada, la lista puede ser nula
                if (vm.MaterialesAsignadosIds != null)
                {
                    foreach (var materialId in vm.MaterialesAsignadosIds)
                    {
                        var nuevaAsignacion = new ProveedorMaterial
                        {
                            ProveedorId = vm.Proveedor.Id,
                            MaterialId = materialId
                        };
                        await _dbContext.ProveedorMateriales.AddAsync(nuevaAsignacion);
                    }
                }

                // Guardamos los cambios de la tabla de unión
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}