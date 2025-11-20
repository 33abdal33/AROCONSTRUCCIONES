using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore; // Necesario para el .Include()
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq; // Necesario para el .OrderByDescending()
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class RequerimientoService : IRequerimientoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RequerimientoService> _logger; // <-- 2. AÑADIR

        public RequerimientoService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RequerimientoService> logger) // <-- 3. AÑADIR
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger; // <-- 4. AÑADIR
        }

        // --- MÉTODO NUEVO: Obtiene todos los requerimientos con el nombre del proyecto ---
        public async Task<IEnumerable<RequerimientoListDto>> GetAllRequerimientosAsync()
        {
            _logger.LogInformation("--- [RequerimientoService] Iniciando GetAllRequerimientosAsync ---");
            try
            {
                var requerimientos = await _unitOfWork.Context.Requerimientos
                    .Include(r => r.Proyecto) // Incluimos la entidad Proyecto
                    .OrderByDescending(r => r.Fecha)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation($"[RequerimientoService] Consulta a BD exitosa. Se encontraron {requerimientos.Count} entidades.");

                var dtos = _mapper.Map<IEnumerable<RequerimientoListDto>>(requerimientos);

                _logger.LogInformation("[RequerimientoService] Mapeo a DTOs exitoso.");
                return dtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RequerimientoService] ERROR en GetAllRequerimientosAsync (BD o Mapeo).");
                throw; // Relanzar para que el controlador lo atrape
            }
        }

        public async Task<IEnumerable<RequerimientoListDto>> GetRequerimientosPorProyectoAsync(int proyectoId)
        {
            var requerimientos = await _unitOfWork.Requerimientos.GetRequerimientosPorProyectoAsync(proyectoId);
            return _mapper.Map<IEnumerable<RequerimientoListDto>>(requerimientos);
        }

        public async Task CreateAsync(RequerimientoCreateDto dto) // Antes: RequerimientoQuickCreateDto
        {
            // 1. Re-activar validación de detalles
            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                throw new ApplicationException("El requerimiento debe tener al menos un material.");
            }

            try
            {
                // 2. Mapear el DTO completo (AutoMapper usará el perfil que corregimos)
                var requerimiento = _mapper.Map<Requerimiento>(dto);

                // 3. Asignar valores por defecto
                requerimiento.Estado = "Pendiente";
                // El 'Codigo' ahora viene desde el DTO/Formulario

                await _unitOfWork.Requerimientos.AddAsync(requerimiento);

                // 4. Guardar Maestro y Detalles en una sola transacción
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw; // Relanza el error
            }
        }
        // --- AÑADIR ESTE NUEVO MÉTODO ---
        public async Task<RequerimientoDetailsDto> GetRequerimientoDetailsAsync(int id)
        {
            _logger.LogInformation($"[RequerimientoService] Obteniendo detalles para Requerimiento ID: {id}");

            // Usamos el método del Repositorio que ya trae todas las relaciones
            var requerimiento = await _unitOfWork.Requerimientos.GetByIdWithDetailsAsync(id);

            if (requerimiento == null)
            {
                _logger.LogWarning($"[RequerimientoService] Requerimiento ID: {id} no encontrado.");
                return null;
            }

            _logger.LogInformation("[RequerimientoService] Mapeando a RequerimientoDetailsDto.");
            return _mapper.Map<RequerimientoDetailsDto>(requerimiento);
        }
        public async Task<bool> ApproveAsync(int id)
        {
            _logger.LogInformation($"[RequerimientoService] Intentando aprobar Requerimiento ID: {id}");

            // 1. Obtener la entidad (¡Necesitamos tracking para actualizarla!)
            // Usamos el Contexto del UoW para traer la entidad CON tracking
            var requerimiento = await _unitOfWork.Context.Requerimientos
                                     .FirstOrDefaultAsync(r => r.Id == id);

            if (requerimiento == null)
            {
                _logger.LogWarning($"[RequerimientoService] Aprobación fallida: ID {id} no encontrado.");
                return false;
            }

            // 2. Lógica de Negocio: Solo se puede aprobar si está "Pendiente"
            if (requerimiento.Estado != "Pendiente")
            {
                _logger.LogWarning($"[RequerimientoService] Aprobación fallida: ID {id} ya está en estado '{requerimiento.Estado}'.");
                // Opcional: podrías lanzar un new ApplicationException("El requerimiento ya fue procesado.");
                return false;
            }

            // 3. Actualizar el estado
            requerimiento.Estado = "Aprobado";

            // 4. Guardar los cambios
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"[RequerimientoService] Requerimiento ID: {id} aprobado exitosamente.");

            return true;
        }
        public async Task<IEnumerable<RequerimientoListDto>> GetAllAprobadosAsync()
        {
            _logger.LogInformation("[RequerimientoService] Obteniendo requerimientos 'Aprobados'.");

            var requerimientos = await _unitOfWork.Context.Requerimientos
                .Include(r => r.Proyecto)
                .Where(r => r.Estado == "Aprobado") // <-- El filtro clave
                .OrderByDescending(r => r.Fecha)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<RequerimientoListDto>>(requerimientos);
        }
    }
}