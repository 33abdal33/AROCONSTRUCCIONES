using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class AlmacenService : IAlmacenService
    {
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO
        private readonly IMapper _mapper;

        // Constructor actualizado: Inyecta IUnitOfWork
        public AlmacenService(IUnitOfWork unitOfWork, IMapper mapper) // <-- CAMBIO
        {
            _unitOfWork = unitOfWork; // <-- CAMBIO
            _mapper = mapper;
        }

        public async Task<IEnumerable<AlmacenDto>> GetAllAsync()
        {
            var almacenes = await _unitOfWork.Almacenes.GetAllAsync();
            return _mapper.Map<IEnumerable<AlmacenDto>>(almacenes);
        }

        public async Task<IEnumerable<AlmacenDto>> GetAllActiveAsync()
        {
            var activos = await _unitOfWork.Almacenes.FindAsync(a => a.Estado == true, a => a.Nombre);
            return _mapper.Map<IEnumerable<AlmacenDto>>(activos);
        }

        public async Task<AlmacenDto?> GetByIdAsync(int id)
        {
            var almacenEntity = await _unitOfWork.Almacenes.GetByIdAsync(id);
            return almacenEntity is null ? null : _mapper.Map<AlmacenDto?>(almacenEntity);
        }

        // --- REFACTORIZADO ---
        public async Task CreateAsync(AlmacenDto dto)
        {
            var entity = _mapper.Map<Almacen>(dto);
            entity.Estado = true;

            await _unitOfWork.Almacenes.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(); // <-- AHORA EL SERVICIO GUARDA
        }

        public async Task<Almacen> UpdateAsync(int id, AlmacenDto dto)
        {
            // OJO: GetByIdAsync para actualizar debe traer la entidad CON tracking
            // Tu RepositoryBase usa AsNoTracking(), necesitarás un método
            // GetByIdWithTrackingAsync(int id) en tu IRepositoryBase.

            // Por ahora, asumiremos que GetByIdAsync() trae la entidad CON tracking:
            var existingEntity = await _unitOfWork.Almacenes.GetByIdAsync(id);
            if (existingEntity == null) return null;

            _mapper.Map(dto, existingEntity);

            // UpdateAsync en RepositoryBase solo marca la entidad como "Modified"
            await _unitOfWork.Almacenes.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync(); // <-- AHORA EL SERVICIO GUARDA
            return existingEntity;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var existing = await _unitOfWork.Almacenes.GetByIdAsync(id);
            if (existing is null) return false;

            existing.Estado = false;

            await _unitOfWork.Almacenes.UpdateAsync(existing);
            await _unitOfWork.SaveChangesAsync(); // <-- AHORA EL SERVICIO GUARDA
            return true;
        }
    }
}