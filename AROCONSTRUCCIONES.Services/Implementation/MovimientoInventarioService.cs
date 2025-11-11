using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System; // Agregado para Exception/ApplicationException
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    // ... (Inyección de dependencias igual)
    public class MovimientoInventarioService : IMovimientoInventarioServices
    {
        private readonly IMovimientoInventarioRepository _movimientoRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IAlmacenRepository _almacenRepository;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public MovimientoInventarioService(
            IMovimientoInventarioRepository movimientoRepository,
            IMaterialRepository materialRepository,
            IAlmacenRepository almacenRepository,
            ApplicationDbContext dbContext,
            IInventarioRepository inventarioRepository,
            IMapper mapper)
        {
            _movimientoRepository = movimientoRepository;
            _materialRepository = materialRepository;
            _almacenRepository = almacenRepository;
            _dbContext = dbContext;
            _inventarioRepository = inventarioRepository;
            _mapper = mapper;
        }

        public async Task<bool> RegistrarIngreso(MovimientoInventarioDto dto)
        {
            // 1. Validaciones
            var material = await _materialRepository.GetByIdAsync(dto.MaterialId);
            var almacen = await _almacenRepository.GetByIdAsync(dto.AlmacenId);

            if (material == null)
            {
                throw new ApplicationException($"El Material con ID {dto.MaterialId} no fue encontrado.");
            }
            if (almacen == null)
            {
                throw new ApplicationException($"El Almacén con ID {dto.AlmacenId} no fue encontrado.");
            }

            // 2. Usar transacción para atomicidad
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var saldo = await _inventarioRepository.FindByKeysAsync(dto.MaterialId, dto.AlmacenId);
                decimal stockFinal;

                // CÁLCULO DE CUPM y ACTUALIZACIÓN DE STOCK
                if (saldo == null)
                {
                    // Primera entrada: CREAR Inventario
                    stockFinal = dto.Cantidad;

                    saldo = new Inventario
                    {
                        MaterialId = dto.MaterialId,
                        AlmacenId = dto.AlmacenId,
                        StockActual = stockFinal,
                        StockMinimo = material.StockMinimo,
                        CostoPromedio = dto.CostoUnitarioCompra,
                        // NO ADJUNTAMOS ENTIDADES RELACIONADAS COMPLETAS (Material o Almacen)
                        // Solo usamos las IDs.
                    };
                    await _inventarioRepository.AddAsync(saldo);
                }
                else
                {
                    // Saldo existente: ACTUALIZAR CUPM y Stock
                    decimal costoActualTotal = saldo.StockActual * saldo.CostoPromedio;
                    decimal costoNuevoTotal = dto.Cantidad * dto.CostoUnitarioCompra;
                    stockFinal = saldo.StockActual + dto.Cantidad;
                    decimal nuevoCostoTotal = costoActualTotal + costoNuevoTotal;

                    if (stockFinal > 0)
                    {
                        saldo.CostoPromedio = nuevoCostoTotal / stockFinal;
                    }

                    saldo.StockActual = stockFinal;
                    saldo.StockMinimo = material.StockMinimo;

                    // 🚨 Punto clave: Solo marcamos 'saldo' como modificado.
                    await _inventarioRepository.UpdateAsync(saldo);
                }

                // 3. Mapear DTO a Entidad de Movimiento y registrar
                var movimiento = _mapper.Map<MovimientoInventario>(dto);
                movimiento.TipoMovimiento = "INGRESO";
                movimiento.FechaMovimiento = DateTime.Now;
                movimiento.CostoUnitarioMovimiento = dto.CostoUnitarioCompra;
                movimiento.StockFinal = stockFinal;
                movimiento.Responsable = dto.ResponsableNombre;

                // Al agregar el movimiento, NINGUNA de sus propiedades de navegación (Material, Almacen, Proveedor)
                // debe tener un objeto completo adjunto, solo las IDs (MaterialId, AlmacenId, ProveedorId).
                // Esto depende de cómo está configurado AutoMapper. Si mapea objetos completos, tenemos que corregir el mapper.

                await _movimientoRepository.AddAsync(movimiento);

                // 4. Guardar y completar la transacción
                // 🚨 La excepción DbUpdateException ocurre aquí si hay una entidad duplicada.
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Si es un error de duplicado (lo que vemos), relanzamos para que el controlador muestre el mensaje.
                throw;
            }
        }

        public async Task<bool> RegistrarSalida(MovimientoInventarioDto dto)
        {
            // 1. Validaciones
            var material = await _materialRepository.GetByIdAsync(dto.MaterialId);
            var almacen = await _almacenRepository.GetByIdAsync(dto.AlmacenId);

            if (material == null)
            {
                throw new ApplicationException($"El Material con ID {dto.MaterialId} no fue encontrado.");
            }
            if (almacen == null)
            {
                throw new ApplicationException($"El Almacén con ID {dto.AlmacenId} no fue encontrado.");
            }

            // 2. Iniciar Transacción
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var saldo = await _inventarioRepository.FindByKeysAsync(dto.MaterialId, dto.AlmacenId);

                if (saldo == null || saldo.StockActual < dto.Cantidad)
                {
                    throw new ApplicationException($"Stock insuficiente en el almacén ({almacen.Nombre}) para la salida solicitada. Stock actual: {saldo?.StockActual ?? 0}.");
                }

                // VALORACIÓN DE SALIDA y Actualización de Stock
                decimal costoUnitarioSalida = saldo.CostoPromedio;
                decimal nuevoStock = saldo.StockActual - dto.Cantidad;

                // 3. Actualizar el Saldo de Inventario
                saldo.StockActual = nuevoStock;
                saldo.StockMinimo = material.StockMinimo;

                await _inventarioRepository.UpdateAsync(saldo);

                // 4. Mapear DTO a Entidad de Movimiento y registrar
                var movimiento = _mapper.Map<MovimientoInventario>(dto);
                movimiento.TipoMovimiento = "SALIDA";
                movimiento.FechaMovimiento = DateTime.Now;
                movimiento.CostoUnitarioMovimiento = costoUnitarioSalida;
                movimiento.StockFinal = nuevoStock;
                movimiento.Responsable = dto.ResponsableNombre;

                await _movimientoRepository.AddAsync(movimiento);

                // 5. Completar la Transacción
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<MovimientoInventarioDto>> GetAllMovimientosAsync()
        {
            var movimientos = await _dbContext.MovimientosInventario
               .Include(m => m.Material)
               .Include(m => m.Almacen)
               .Include(m => m.Proveedor)
               .OrderByDescending(m => m.FechaMovimiento)
               .ToListAsync();

            return _mapper.Map<IEnumerable<MovimientoInventarioDto>>(movimientos);
        }
        public async Task<IEnumerable<MovimientoInventarioDto>> GetHistorialPorMaterialYAlmacenAsync(int materialId, int almacenId)
        {
            // 1. Buscamos en la tabla de Movimientos
            var movimientos = await _dbContext.MovimientosInventario
                .Include(m => m.Material) // Incluimos Material para el DTO
                .Include(m => m.Almacen)  // Incluimos Almacen para el DTO
                .Include(m => m.Proveedor) // Incluimos Proveedor (si existe)

                // 2. Filtramos por el Material Y el Almacén
                .Where(m => m.MaterialId == materialId && m.AlmacenId == almacenId)

                // 3. Ordenamos por fecha (el más reciente primero)
                .OrderByDescending(m => m.FechaMovimiento)

                .ToListAsync();

            // 4. Mapeamos las entidades a DTOs y devolvemos
            return _mapper.Map<IEnumerable<MovimientoInventarioDto>>(movimientos);
        }
    }

}
