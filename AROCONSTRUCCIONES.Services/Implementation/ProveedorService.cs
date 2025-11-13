using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces; // <-- CAMBIO
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore; // <-- Para DbUpdateException
using Microsoft.Data.SqlClient; // <-- Para SqlException
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class ProveedorService : IProveedorService
    {
        // --- CAMBIOS EN DEPENDENCIAS ---
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMaterialServices _materialService; // Para leer categorías
        private readonly IMapper _mapper;

        public ProveedorService(
          IUnitOfWork unitOfWork, // <-- CAMBIO
          IMaterialServices materialService,
          IMapper mapper)
        {
            _unitOfWork = unitOfWork; // <-- CAMBIO
            _materialService = materialService;
            _mapper = mapper;
        }

        // --- MÉTODOS DE LECTURA (usando _unitOfWork) ---

        public async Task<IEnumerable<ProveedorDto>> GetAllAsync()
        {
            var proveedores = await _unitOfWork.Proveedores.GetAllAsync();
            return _mapper.Map<IEnumerable<ProveedorDto>>(proveedores);
        }

        public async Task<IEnumerable<ProveedorDto>> GetAllActiveAsync()
        {
            var activos = await _unitOfWork.Proveedores.FindAsync(p => p.Estado == true, p => p.RazonSocial);
            return _mapper.Map<IEnumerable<ProveedorDto>>(activos);
        }

        public async Task<ProveedorDto?> GetByIdAsync(int id)
        {
            var proveedorEntity = await _unitOfWork.Proveedores.GetByIdAsync(id);
            if (proveedorEntity is null) return null;
            return _mapper.Map<ProveedorDto>(proveedorEntity);
        }

        public async Task<ProveedorEdicionViewModelDto> GetEdicionProveedorAsync(int id)
        {
            var vm = new ProveedorEdicionViewModelDto();
            vm.CategoriasMateriales = await _materialService.GetMaterialCategoriesAsync();

            // Usamos el UoW para acceder al repo de materiales
            var todosLosMateriales = await _unitOfWork.Materiales.GetAllAsync();
            vm.TodosLosMateriales = _mapper.Map<List<MaterialDto>>(todosLosMateriales);

            if (id == 0) return vm;

            vm.Proveedor = await GetByIdAsync(id);
            if (vm.Proveedor == null) throw new ApplicationException("Proveedor no encontrado");

            // Usamos el Context del UoW para la tabla de unión
            vm.MaterialesAsignadosIds = await _unitOfWork.Context.ProveedorMateriales
                .Where(pm => pm.ProveedorId == id)
                .Select(pm => pm.MaterialId)
                .ToListAsync();

            return vm;
        }

        // --- MÉTODOS DE ESCRITURA (Ahora con UoW y manejo de errores) ---

        public async Task CreateAsync(ProveedorEdicionViewModelDto vm)
        {
            try
            {
                var entity = _mapper.Map<Proveedor>(vm.Proveedor);
                entity.Estado = true;
                await _unitOfWork.Proveedores.AddAsync(entity);

                // Asignar materiales (si los hay)
                if (vm.MaterialesAsignadosIds != null)
                {
                    foreach (var materialId in vm.MaterialesAsignadosIds)
                    {
                        // Creamos la relación. EF Core la asociará al 'entity' en memoria.
                        var nuevaAsignacion = new ProveedorMaterial
                        {
                            Proveedor = entity, // Asocia la entidad en memoria
                            MaterialId = materialId
                        };
                        // Añadimos al DbContext a través del UoW
                        await _unitOfWork.Context.ProveedorMateriales.AddAsync(nuevaAsignacion);
                    }
                }

                // Guardamos TODO (Proveedor y ProveedorMateriales) en una sola transacción
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
                {
                    throw new ApplicationException("Error: Ya existe un proveedor con ese RUC.");
                }
                throw; // Lanza otra excepción de BD
            }
        }

        // Este método se queda igual, solo que no devuelve nada
        public async Task<Proveedor> UpdateAsync(int id, ProveedorDto dto)
        {
            var existing = await _unitOfWork.Proveedores.GetByIdAsync(id);
            if (existing is null) return null;
            _mapper.Map(dto, existing);
            await _unitOfWork.Proveedores.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync(); // <-- Guarda
            return existing;
        }

        // Este método ya estaba casi bien, solo reemplazamos el DbContext por el UoW
        public async Task UpdateProveedorCompletoAsync(ProveedorEdicionViewModelDto vm)
        {
            try
            {
                // 1. Guardar los datos del Proveedor
                var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(vm.Proveedor.Id);
                if (proveedor == null)
                    throw new ApplicationException("Proveedor no encontrado.");

                _mapper.Map(vm.Proveedor, proveedor);
                await _unitOfWork.Proveedores.UpdateAsync(proveedor);

                // 2. Sincronizar los Materiales Asignados
                var asignacionesViejas = _unitOfWork.Context.ProveedorMateriales
                    .Where(pm => pm.ProveedorId == vm.Proveedor.Id);

                _unitOfWork.Context.ProveedorMateriales.RemoveRange(asignacionesViejas);

                if (vm.MaterialesAsignadosIds != null)
                {
                    foreach (var materialId in vm.MaterialesAsignadosIds)
                    {
                        var nuevaAsignacion = new ProveedorMaterial
                        {
                            ProveedorId = vm.Proveedor.Id,
                            MaterialId = materialId
                        };
                        await _unitOfWork.Context.ProveedorMateriales.AddAsync(nuevaAsignacion);
                    }
                }

                // Guardamos TODO (Proveedor actualizado y ProveedorMateriales) en una sola transacción
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
                {
                    throw new ApplicationException("Error: El RUC ingresado ya pertenece a otro proveedor.");
                }
                throw;
            }
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var existing = await _unitOfWork.Proveedores.GetByIdAsync(id);
            if (existing is null) return false;

            existing.Estado = false;
            await _unitOfWork.Proveedores.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync(); // <-- Guarda
            return true;
        }
    }
}