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
    public class MovimientoInventarioService : IMovimientoInventarioServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MovimientoInventarioService> _logger;

        public MovimientoInventarioService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<MovimientoInventarioService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> RegistrarIngreso(MovimientoInventarioDto dto)
        {
            // 1. Validaciones
            var material = await _unitOfWork.Materiales.GetByIdAsync(dto.MaterialId);
            if (material == null)
                throw new ApplicationException($"El Material con ID {dto.MaterialId} no fue encontrado.");

            var almacen = await _unitOfWork.Almacenes.GetByIdAsync(dto.AlmacenId);
            if (almacen == null)
                throw new ApplicationException($"El Almacén con ID {dto.AlmacenId} no fue encontrado.");

            // 2. Lógica de Costo Promedio Ponderado (CPP)
            try
            {
                var saldo = await _unitOfWork.Inventario.FindByKeysAsync(dto.MaterialId, dto.AlmacenId);
                decimal stockFinal;

                if (saldo == null)
                {
                    stockFinal = dto.Cantidad;
                    saldo = new Inventario
                    {
                        MaterialId = dto.MaterialId,
                        AlmacenId = dto.AlmacenId,
                        StockActual = stockFinal,
                        StockMinimo = material.StockMinimo,
                        CostoPromedio = dto.CostoUnitarioCompra,
                        FechaUltimoMovimiento = dto.FechaMovimiento,
                        NivelAlerta = 0
                    };
                    await _unitOfWork.Inventario.AddAsync(saldo);
                }
                else
                {
                    decimal costoActualTotal = saldo.StockActual * saldo.CostoPromedio;
                    decimal costoNuevoTotal = dto.Cantidad * dto.CostoUnitarioCompra;
                    stockFinal = saldo.StockActual + dto.Cantidad;

                    // Nuevo Precio Promedio
                    if (stockFinal > 0)
                        saldo.CostoPromedio = (costoActualTotal + costoNuevoTotal) / stockFinal;

                    saldo.StockActual = stockFinal;
                    saldo.StockMinimo = material.StockMinimo;
                    saldo.FechaUltimoMovimiento = dto.FechaMovimiento;

                    await _unitOfWork.Inventario.UpdateAsync(saldo);
                }

                // 3. Registrar el Movimiento
                var movimiento = _mapper.Map<MovimientoInventario>(dto);
                movimiento.TipoMovimiento = "INGRESO";
                movimiento.CostoUnitarioMovimiento = dto.CostoUnitarioCompra;
                movimiento.StockFinal = stockFinal;
                movimiento.Responsable = dto.ResponsableNombre;

                await _unitOfWork.MovimientosInventario.AddAsync(movimiento);

                // 4. Guardar cambios
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RegistrarSalida(MovimientoInventarioDto dto)
        {
            _logger.LogInformation("Iniciando RegistrarSalida...");

            // 1. Validaciones Básicas
            var material = await _unitOfWork.Materiales.GetByIdAsync(dto.MaterialId);
            if (material == null) throw new ApplicationException("Material no encontrado.");

            var almacen = await _unitOfWork.Almacenes.GetByIdAsync(dto.AlmacenId);
            if (almacen == null) throw new ApplicationException("Almacén no encontrado.");

            DateTime fechaMovimiento = dto.FechaMovimiento == default ? DateTime.Now : dto.FechaMovimiento;

            // Validación de Proyecto
            if (dto.Motivo == "CONSUMO_PROYECTO" && (!dto.IdProyecto.HasValue || dto.IdProyecto == 0))
            {
                throw new ApplicationException("Para 'Consumo Proyecto', es obligatorio seleccionar un proyecto.");
            }

            // Usamos transacción para asegurar consistencia entre Inventario, Movimiento y Costos del Proyecto
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                var saldo = await _unitOfWork.Inventario.FindByKeysAsync(dto.MaterialId, dto.AlmacenId);

                if (saldo == null || saldo.StockActual < dto.Cantidad)
                {
                    throw new ApplicationException($"Stock insuficiente en {almacen.Nombre}. Stock actual: {saldo?.StockActual.ToString("N2") ?? "0"}.");
                }

                decimal costoUnitarioSalida = saldo.CostoPromedio;
                decimal nuevoStock = saldo.StockActual - dto.Cantidad;
                decimal costoTotalSalida = dto.Cantidad * costoUnitarioSalida; // Costo real a imputar

                // 2. Actualizar Stock
                saldo.StockActual = nuevoStock;
                saldo.StockMinimo = material.StockMinimo;
                saldo.FechaUltimoMovimiento = fechaMovimiento;
                await _unitOfWork.Inventario.UpdateAsync(saldo);

                // 3. Registrar Movimiento
                var movimiento = _mapper.Map<MovimientoInventario>(dto);
                movimiento.TipoMovimiento = "SALIDA";
                movimiento.FechaMovimiento = fechaMovimiento;
                movimiento.CostoUnitarioMovimiento = costoUnitarioSalida; // Sale valorizado al promedio
                movimiento.StockFinal = nuevoStock;
                movimiento.Responsable = dto.ResponsableNombre;

                // Mapear manualmente para asegurar (aunque AutoMapper debería hacerlo si los nombres coinciden)
                movimiento.PartidaId = dto.PartidaId;

                await _unitOfWork.MovimientosInventario.AddAsync(movimiento);

                // 4. Actualizar Costos del Proyecto y Partida (Saldos Presupuestales)
                if (dto.IdProyecto.HasValue && dto.Motivo == "CONSUMO_PROYECTO")
                {
                    // A. Actualizar Costo Global del Proyecto
                    var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(dto.IdProyecto.Value);
                    if (proyecto != null)
                    {
                        proyecto.CostoEjecutado += costoTotalSalida;
                        await _unitOfWork.Proyectos.UpdateAsync(proyecto);
                    }

                    // B. Actualizar Costo Específico de la Partida
                    if (dto.PartidaId.HasValue && dto.PartidaId > 0)
                    {
                        // Accedemos al DbSet de Partidas a través del Contexto del UoW
                        var partida = await _unitOfWork.Context.Set<Partida>().FindAsync(dto.PartidaId.Value);
                        if (partida != null)
                        {
                            partida.CostoEjecutado += costoTotalSalida;
                            _unitOfWork.Context.Entry(partida).State = EntityState.Modified; // Marcar para update
                        }
                    }
                }

                // 5. Commit de la transacción
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error en RegistrarSalida");
                throw;
            }
        }

        public async Task<IEnumerable<MovimientoInventarioDto>> GetAllMovimientosAsync()
        {
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