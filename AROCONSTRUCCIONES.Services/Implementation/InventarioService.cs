using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class InventarioService : IInventarioService
    {
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO
        private readonly IMapper _mapper;

        public InventarioService(IUnitOfWork unitOfWork, IMapper mapper) // <-- CAMBIO
        {
            _unitOfWork = unitOfWork; // <-- CAMBIO
            _mapper = mapper;
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
    }
}
