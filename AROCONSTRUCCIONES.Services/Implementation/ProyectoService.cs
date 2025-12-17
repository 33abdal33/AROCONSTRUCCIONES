using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class ProyectoService : IProyectoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProyectoService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProyectoDto>> GetAllAsync()
        {
            var proyectos = await _unitOfWork.Proyectos.GetAllAsync();
            return _mapper.Map<IEnumerable<ProyectoDto>>(proyectos);
        }

        // <--- IMPLEMENTACIÓN AGREGADA ---
        public async Task<IEnumerable<ProyectoDto>> GetAllProyectosAsync()
        {
            // Hacemos exactamente lo mismo que GetAllAsync
            var proyectos = await _unitOfWork.Proyectos.GetAllAsync();
            return _mapper.Map<IEnumerable<ProyectoDto>>(proyectos);
        }
        // --------------------------------

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
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Proyecto> UpdateAsync(int id, ProyectoDto dto)
        {
            var entity = await _unitOfWork.Proyectos.GetByIdAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);

            await _unitOfWork.Proyectos.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }
    }
}