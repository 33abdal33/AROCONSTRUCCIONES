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
        private readonly IAlmacenRepository _almacenRepository;
        private readonly IMapper _mapper;
        // DbContext se elimina, ya no guarda.

        public AlmacenService(IAlmacenRepository almacenRepository, IMapper mapper)
        {
            _almacenRepository = almacenRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AlmacenDto>> GetAllAsync()
        {
            var almacenes = await _almacenRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AlmacenDto>>(almacenes);
        }

        public async Task<IEnumerable<AlmacenDto>> GetAllActiveAsync()
        {
            var activos = await _almacenRepository.FindAsync(a => a.Estado == true, a => a.Nombre);
            return _mapper.Map<IEnumerable<AlmacenDto>>(activos);
        }

        public async Task<AlmacenDto?> GetByIdAsync(int id)
        {
            var almacenEntity = await _almacenRepository.GetByIdAsync(id);
            return almacenEntity is null ? null : _mapper.Map<AlmacenDto?>(almacenEntity);
        }

        // --- REFACTORIZADO ---
        public async Task CreateAsync(AlmacenDto dto)
        {
            var entity = _mapper.Map<Almacen>(dto);
            entity.Estado = true; // Asegurar que esté activo al crear
            await _almacenRepository.AddAsync(entity);
            // SaveChanges() ELIMINADO
        }

        // --- REFACTORIZADO ---
        public async Task<Almacen> UpdateAsync(int id, AlmacenDto dto)
        {
            var existingEntity = await _almacenRepository.GetByIdAsync(id);
            if (existingEntity == null) return null;

            _mapper.Map(dto, existingEntity);
            await _almacenRepository.UpdateAsync(existingEntity);
            // SaveChanges() ELIMINADO
            return existingEntity;
        }

        // --- REFACTORIZADO ---
        // Implementa DeactivateAsync (Soft Delete)
        public async Task<bool> DeactivateAsync(int id)
        {
            var existing = await _almacenRepository.GetByIdAsync(id);
            if (existing is null) return false;

            existing.Estado = false; // Soft Delete
            await _almacenRepository.UpdateAsync(existing);
            // SaveChanges() ELIMINADO
            return true;
        }
    }
}