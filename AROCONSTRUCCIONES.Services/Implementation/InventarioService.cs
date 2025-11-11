using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class InventarioService : IInventarioService
    {
        private readonly IInventarioRepository _inventarioRepository;
        private readonly IMapper _mapper;

        public InventarioService(IInventarioRepository inventarioRepository, IMapper mapper)
        {
            _inventarioRepository = inventarioRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InventarioDto>> GetAllStockViewAsync()
        {
            // 1. Obtener las Entidades desde el Repositorio.
            // El repositorio ya se encarga del .Include(Material) y .Include(Almacen)
            var inventarioEntidades = await _inventarioRepository.GetAllAsync();

            // 2. Mapear la colección de Entidades a DTOs (usando el InventarioProfile que creaste)
            var inventarioDtos = _mapper.Map<IEnumerable<InventarioDto>>(inventarioEntidades);

            // NOTA: AutoMapper se encarga de calcular el ValorTotal y el EstadoTexto si lo configuraste.

            return inventarioDtos;
        }

        public async Task<InventarioDto?> GetStockByKeysAsync(int materialId, int almacenId)
        {
            var inventarioEntidad = await _inventarioRepository.FindByKeysAsync(materialId, almacenId);

            if (inventarioEntidad == null)
            {
                return null;
            }

            // Mapear la entidad individual al DTO
            return _mapper.Map<InventarioDto>(inventarioEntidad);
        }
    }
}
