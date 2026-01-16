using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class InventarioService : IInventarioService
    {
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO
        private readonly IMapper _mapper;
        private readonly ILogger<InventarioService> _logger;

        public InventarioService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<InventarioService> logger) // <-- CAMBIO
        {
            _unitOfWork = unitOfWork; // <-- CAMBIO
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<InventarioDto>> GetAllStockViewAsync()
        {
            // Usa el repositorio de Inventario desde el Unit of Work
            var inventarioEntidades = await _unitOfWork.Inventario.GetAllAsync(); // <-- CAMBIO
            var inventarioDtos = _mapper.Map<IEnumerable<InventarioDto>>(inventarioEntidades);
            return inventarioDtos;
        }

        public async Task<InventarioDto?> GetStockByKeysAsync(int materialId, int almacenId)
        {
            // Usa el repositorio de Inventario desde el Unit of Work
            var inventarioEntidad = await _unitOfWork.Inventario.FindByKeysAsync(materialId, almacenId); // <-- CAMBIO

            if (inventarioEntidad == null)
            {
                return null;
            }
            return _mapper.Map<InventarioDto>(inventarioEntidad);
        }
        public async Task<IEnumerable<StockPorAlmacenDto>> GetStockActualPorAlmacenAsync(int almacenId)
        {
            _logger.LogInformation($"[InventarioService] Consultando stock físico para Almacén ID: {almacenId}");

            return await _unitOfWork.Context.Inventario
                .Where(i => i.AlmacenId == almacenId)
                .Select(i => new StockPorAlmacenDto
                {
                    MaterialId = i.MaterialId,
                    Stock = i.StockActual
                })
                .AsNoTracking() // Mejora el rendimiento al ser solo lectura
                .ToListAsync();
        }
    }
}
