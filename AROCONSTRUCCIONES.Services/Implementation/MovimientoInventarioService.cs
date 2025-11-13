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
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO
        private readonly IMapper _mapper;

        public MovimientoInventarioService(
            IUnitOfWork unitOfWork, // <-- CAMBIO
            IMapper mapper)
        {
            _unitOfWork = unitOfWork; // <-- CAMBIO
            _mapper = mapper;
        }

        public async Task<bool> RegistrarIngreso(MovimientoInventarioDto dto)
        {
            // 1. Validaciones usando el UoW
            var material = await _unitOfWork.Materiales.GetByIdAsync(dto.MaterialId);
            if (material == null)
                throw new ApplicationException($"El Material con ID {dto.MaterialId} no fue encontrado.");

            var almacen = await _unitOfWork.Almacenes.GetByIdAsync(dto.AlmacenId);
            if (almacen == null)
                throw new ApplicationException($"El Almacén con ID {dto.AlmacenId} no fue encontrado.");

            // 2. Transacción DESAPARECE. SaveChangesAsync() es atómico por defecto.
            try
            {
                var saldo = await _unitOfWork.Inventario.FindByKeysAsync(dto.MaterialId, dto.AlmacenId);
                decimal stockFinal;

                if (saldo == null)
                {
                    // ... (lógica de crear inventario)
                    stockFinal = dto.Cantidad;
                    saldo = new Inventario
                    {
                        MaterialId = dto.MaterialId,
                        AlmacenId = dto.AlmacenId,
                        StockActual = stockFinal,
                        StockMinimo = material.StockMinimo,
                        CostoPromedio = dto.CostoUnitarioCompra,
                        FechaUltimoMovimiento = dto.FechaMovimiento
                    };
                    await _unitOfWork.Inventario.AddAsync(saldo); // <-- Usa UoW
                }
                else
                {
                    // ... (lógica de actualizar CUPM)
                    decimal costoActualTotal = saldo.StockActual * saldo.CostoPromedio;
                    decimal costoNuevoTotal = dto.Cantidad * dto.CostoUnitarioCompra;
                    stockFinal = saldo.StockActual + dto.Cantidad;
                    decimal nuevoCostoTotal = costoActualTotal + costoNuevoTotal;

                    if (stockFinal > 0)
                        saldo.CostoPromedio = nuevoCostoTotal / stockFinal;

                    saldo.StockActual = stockFinal;
                    saldo.StockMinimo = material.StockMinimo;
                    saldo.FechaUltimoMovimiento = dto.FechaMovimiento;

                    await _unitOfWork.Inventario.UpdateAsync(saldo); // <-- Usa UoW
                }

                // 3. Mapear y registrar el movimiento
                var movimiento = _mapper.Map<MovimientoInventario>(dto);
                movimiento.TipoMovimiento = "INGRESO";
                movimiento.CostoUnitarioMovimiento = dto.CostoUnitarioCompra;
                movimiento.StockFinal = stockFinal;
                movimiento.Responsable = dto.ResponsableNombre;

                await _unitOfWork.MovimientosInventario.AddAsync(movimiento); // <-- Usa UoW

                // 4. Guardar TODO (Inventario + Movimiento) en una sola transacción
                await _unitOfWork.SaveChangesAsync(); // <-- CAMBIO

                return true;
            }
            catch (Exception)
            {
                // No hay 'transaction.RollbackAsync()'. 
                // Si SaveChangesAsync falla, EF Core lo hace automáticamente.
                throw;
            }
        }

        public async Task<bool> RegistrarSalida(MovimientoInventarioDto dto)
        {
            // 1. Validaciones
            var material = await _unitOfWork.Materiales.GetByIdAsync(dto.MaterialId);
            if (material == null)
                throw new ApplicationException("Material no encontrado.");

            var almacen = await _unitOfWork.Almacenes.GetByIdAsync(dto.AlmacenId);
            if (almacen == null)
                throw new ApplicationException("Almacén no encontrado.");

            try
            {
                var saldo = await _unitOfWork.Inventario.FindByKeysAsync(dto.MaterialId, dto.AlmacenId);

                if (saldo == null || saldo.StockActual < dto.Cantidad)
                {
                    throw new ApplicationException($"Stock insuficiente. Stock actual: {saldo?.StockActual ?? 0}.");
                }

                decimal costoUnitarioSalida = saldo.CostoPromedio;
                decimal nuevoStock = saldo.StockActual - dto.Cantidad;

                // 3. Actualizar el Saldo
                saldo.StockActual = nuevoStock;
                saldo.StockMinimo = material.StockMinimo;
                saldo.FechaUltimoMovimiento = dto.FechaMovimiento;
                await _unitOfWork.Inventario.UpdateAsync(saldo); // <-- Usa UoW

                // 4. Registrar el Movimiento
                var movimiento = _mapper.Map<MovimientoInventario>(dto);
                movimiento.TipoMovimiento = "SALIDA";
                movimiento.CostoUnitarioMovimiento = costoUnitarioSalida;
                movimiento.StockFinal = nuevoStock;
                movimiento.Responsable = dto.ResponsableNombre;

                await _unitOfWork.MovimientosInventario.AddAsync(movimiento); // <-- Usa UoW

                // 5. Completar la Transacción
                await _unitOfWork.SaveChangesAsync(); // <-- CAMBIO

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<MovimientoInventarioDto>> GetAllMovimientosAsync()
        {
            // Para consultas complejas con "Include", es válido usar el Context del UoW
            var movimientos = await _unitOfWork.Context.MovimientosInventario
               .Include(m => m.Material)
               .Include(m => m.Almacen)
               .Include(m => m.Proveedor)
               .OrderByDescending(m => m.FechaMovimiento)
               .ToListAsync();

            return _mapper.Map<IEnumerable<MovimientoInventarioDto>>(movimientos);
        }
        public async Task<IEnumerable<MovimientoInventarioDto>> GetHistorialPorMaterialYAlmacenAsync(int materialId, int almacenId)
        {
            var movimientos = await _unitOfWork.Context.MovimientosInventario
                .Include(m => m.Material)
                .Include(m => m.Almacen)
                .Include(m => m.Proveedor)
                .Where(m => m.MaterialId == materialId && m.AlmacenId == almacenId)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();

            return _mapper.Map<IEnumerable<MovimientoInventarioDto>>(movimientos);
        }
    }

}
