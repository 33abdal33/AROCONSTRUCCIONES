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
        public async Task<string> GetNextCodigoAsync()
        {
            _logger.LogInformation("[RequerimientoService] Generando siguiente código correlativo.");

            // Buscamos el último requerimiento creado
            var ultimo = await _unitOfWork.Context.Requerimientos
                .OrderByDescending(r => r.Id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            string prefijo = "REQ-";
            int siguienteNumero = 1;

            if (ultimo != null && !string.IsNullOrEmpty(ultimo.Codigo))
            {
                // Intentamos extraer el número después del guion (ej. REQ-0005 -> 0005)
                string parteNumerica = ultimo.Codigo.Replace(prefijo, "");
                if (int.TryParse(parteNumerica, out int ultimoNumero))
                {
                    siguienteNumero = ultimoNumero + 1;
                }
            }

            // Retorna con formato de 4 dígitos: REQ-0001
            return $"{prefijo}{siguienteNumero.ToString("D4")}";
        }

        public async Task CreateAsync(RequerimientoCreateDto dto)
        {
            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                throw new ApplicationException("El requerimiento debe tener al menos un material.");
            }

            try
            {
                // IMPORTANTE: Volvemos a generar el código aquí para evitar duplicados 
                // si dos usuarios abrieron el modal al mismo tiempo.
                dto.Codigo = await GetNextCodigoAsync();

                var requerimiento = _mapper.Map<Requerimiento>(dto);

                requerimiento.Estado = "Pendiente";
                if (requerimiento.FechaSolicitud == default)
                    requerimiento.FechaSolicitud = DateTime.Now;

                await _unitOfWork.Requerimientos.AddAsync(requerimiento);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"[RequerimientoService] Requerimiento {requerimiento.Codigo} creado con éxito.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RequerimientoService] Error al crear el requerimiento.");
                throw;
            }
        }
        public async Task<bool> CancelAsync(int id)
        {
            _logger.LogInformation($"[RequerimientoService] Intentando cancelar Requerimiento ID: {id}");

            var requerimiento = await _unitOfWork.Context.Requerimientos
                                                 .FirstOrDefaultAsync(r => r.Id == id);

            if (requerimiento == null)
            {
                _logger.LogWarning($"[RequerimientoService] Cancelación fallida: ID {id} no encontrado.");
                return false;
            }

            // Solo permitimos cancelar si está Pendiente
            if (requerimiento.Estado != "Pendiente")
            {
                _logger.LogWarning($"[RequerimientoService] No se puede cancelar el ID {id} porque está en estado '{requerimiento.Estado}'.");
                return false;
            }

            requerimiento.Estado = "Cancelado";
            // Opcional: registrar fecha de cancelación si tienes el campo
            // requerimiento.FechaCancelacion = DateTime.Now;

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"[RequerimientoService] Requerimiento ID: {id} cancelado correctamente.");

            return true;
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