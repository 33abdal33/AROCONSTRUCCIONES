using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class RequerimientoService : IRequerimientoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RequerimientoService> _logger;

        public RequerimientoService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<RequerimientoService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // --- MÉTODO CORREGIDO ---
        public async Task<IEnumerable<RequerimientoListDto>> GetAllRequerimientosAsync()
        {
            _logger.LogInformation("--- [RequerimientoService] Iniciando GetAllRequerimientosAsync ---");
            try
            {
                var requerimientos = await _unitOfWork.Context.Requerimientos
                    .Include(r => r.Proyecto)
                    // CORRECCIÓN: Usamos FechaSolicitud en lugar de Fecha
                    .OrderByDescending(r => r.FechaSolicitud)
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
                throw;
            }
        }

        public async Task<IEnumerable<RequerimientoListDto>> GetRequerimientosPorProyectoAsync(int proyectoId)
        {
            // Nota: Asegúrate de que tu Repositorio también use FechaSolicitud (ya lo corregimos en el paso anterior)
            var requerimientos = await _unitOfWork.Requerimientos.GetRequerimientosPorProyectoAsync(proyectoId);
            return _mapper.Map<IEnumerable<RequerimientoListDto>>(requerimientos);
        }

        public async Task CreateAsync(RequerimientoCreateDto dto)
        {
            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                throw new ApplicationException("El requerimiento debe tener al menos un material.");
            }

            try
            {
                var requerimiento = _mapper.Map<Requerimiento>(dto);

                // Asignar valores por defecto si no vienen
                requerimiento.Estado = "Pendiente";
                if (requerimiento.FechaSolicitud == default)
                    requerimiento.FechaSolicitud = DateTime.Now;

                await _unitOfWork.Requerimientos.AddAsync(requerimiento);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<RequerimientoDetailsDto> GetRequerimientoDetailsAsync(int id)
        {
            _logger.LogInformation($"[RequerimientoService] Obteniendo detalles para Requerimiento ID: {id}");

            var requerimiento = await _unitOfWork.Requerimientos.GetByIdWithDetailsAsync(id);

            if (requerimiento == null)
            {
                _logger.LogWarning($"[RequerimientoService] Requerimiento ID: {id} no encontrado.");
                return null;
            }

            return _mapper.Map<RequerimientoDetailsDto>(requerimiento);
        }

        public async Task<bool> ApproveAsync(int id)
        {
            _logger.LogInformation($"[RequerimientoService] Intentando aprobar Requerimiento ID: {id}");

            var requerimiento = await _unitOfWork.Context.Requerimientos
                                         .FirstOrDefaultAsync(r => r.Id == id);

            if (requerimiento == null)
            {
                _logger.LogWarning($"[RequerimientoService] Aprobación fallida: ID {id} no encontrado.");
                return false;
            }

            if (requerimiento.Estado != "Pendiente")
            {
                _logger.LogWarning($"[RequerimientoService] Aprobación fallida: ID {id} ya está en estado '{requerimiento.Estado}'.");
                return false;
            }

            requerimiento.Estado = "Aprobado";

            // Opcional: Guardar quién lo aprobó si implementaste ese campo
            // requerimiento.UsuarioAprobador = "UsuarioActual"; 
            // requerimiento.FechaAprobacion = DateTime.Now;

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"[RequerimientoService] Requerimiento ID: {id} aprobado exitosamente.");

            return true;
        }

        // --- MÉTODO CORREGIDO ---
        public async Task<IEnumerable<RequerimientoListDto>> GetAllAprobadosAsync()
        {
            _logger.LogInformation("[RequerimientoService] Obteniendo requerimientos 'Aprobados'.");

            var requerimientos = await _unitOfWork.Context.Requerimientos
                .Include(r => r.Proyecto)
                .Where(r => r.Estado == "Aprobado")
                // CORRECCIÓN: Usamos FechaSolicitud en lugar de Fecha
                .OrderByDescending(r => r.FechaSolicitud)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<RequerimientoListDto>>(requerimientos);
        }
    }
}