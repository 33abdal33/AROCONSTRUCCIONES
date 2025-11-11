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
        private readonly IProyectoRepository _proyectoRepo;
        private readonly IMapper _mapper;

        public ProyectoService(IProyectoRepository proyectoRepo, IMapper mapper)
        {
            _proyectoRepo = proyectoRepo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProyectoDto>> GetAllAsync()
        {
            var proyectos = await _proyectoRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<ProyectoDto>>(proyectos);
        }

        public async Task<ProyectoDto?> GetByIdAsync(int id)
        {
            var proyecto = await _proyectoRepo.GetByIdAsync(id);
            if (proyecto == null) return null;
            return _mapper.Map<ProyectoDto>(proyecto);
        }

        public async Task CreateAsync(ProyectoDto dto)
        {
            var entity = _mapper.Map<Proyecto>(dto);

            // Asignamos valores por defecto que no vienen del formulario
            entity.Estado = "Planificación";
            entity.AvancePorcentaje = 0;
            entity.CostoEjecutado = 0;

            await _proyectoRepo.AddAsync(entity);
            // Sin SaveChanges()
        }

        public async Task<Proyecto> UpdateAsync(int id, ProyectoDto dto)
        {
            // Para 'Update', GetByIdAsync debe usar tracking.
            // Tu RepositoryBase usa AsNoTracking() por defecto.
            // Debemos pedirle al repositorio la entidad CON tracking.
            // Asumiremos que el GetByIdAsync del repo base NO usa AsNoTracking.
            // OJO: Si tu RepositoryBase.GetByIdAsync() usa AsNoTracking(), esta lógica debe cambiar.

            var entity = await _proyectoRepo.GetByIdAsync(id); // Asumimos que esto rastrea la entidad
            if (entity == null) return null;

            // Mapea los cambios del DTO a la entidad que ya está siendo rastreada
            _mapper.Map(dto, entity);

            await _proyectoRepo.UpdateAsync(entity); // Marca la entidad como modificada
            // Sin SaveChanges()
            return entity;
        }
    }
}