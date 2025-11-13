using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces; // <-- CAMBIO
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class ProyectoService : IProyectoService
    {
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO
        private readonly IMapper _mapper;

        public ProyectoService(IUnitOfWork unitOfWork, IMapper mapper) // <-- CAMBIO
        {
            _unitOfWork = unitOfWork; // <-- CAMBIO
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProyectoDto>> GetAllAsync()
        {
            var proyectos = await _unitOfWork.Proyectos.GetAllAsync();
            return _mapper.Map<IEnumerable<ProyectoDto>>(proyectos);
        }

        public async Task<ProyectoDto?> GetByIdAsync(int id)
        {
            var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(id);
            if (proyecto == null) return null;
            return _mapper.Map<ProyectoDto>(proyecto);
        }

        public async Task CreateAsync(ProyectoDto dto)
        {
            var entity = _mapper.Map<Proyecto>(dto);
            entity.Estado = "Planificación";
            entity.AvancePorcentaje = 0;
            entity.CostoEjecutado = 0;

            await _unitOfWork.Proyectos.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(); // <-- AHORA GUARDA EL SERVICIO
        }

        public async Task<Proyecto> UpdateAsync(int id, ProyectoDto dto)
        {
            // Usamos GetByIdAsync (que es NoTracking)
            var entity = await _unitOfWork.Proyectos.GetByIdAsync(id);
            if (entity == null) return null;

            // Mapeamos los cambios del DTO a la entidad desconectada
            _mapper.Map(dto, entity);

            // Usamos el UpdateAsync del Repositorio (que adjunta y marca como Modificado)
            await _unitOfWork.Proyectos.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync(); // <-- AHORA GUARDA EL SERVICIO
            return entity;
        }
    }
}