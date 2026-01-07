using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                // A. Validaciones iniciales
                var material = await _unitOfWork.Materiales.GetByIdAsync(dto.MaterialId);
                if (material == null) throw new ApplicationException("Material no encontrado.");

                var almacen = await _unitOfWork.Almacenes.GetByIdAsync(dto.AlmacenId);
                if (almacen == null) throw new ApplicationException("Almacén no encontrado.");

                // B. TRAZABILIDAD: Si viene de una Orden de Compra
                if (dto.DetalleOrdenCompraId.HasValue)
                {
                    var detalleOC = await _unitOfWork.Context.Set<DetalleOrdenCompra>()
                        .Include(d => d.OrdenCompra)
                        .FirstOrDefaultAsync(d => d.Id == dto.DetalleOrdenCompraId.Value);

                    if (detalleOC != null)
                    {
                        decimal pendiente = detalleOC.Cantidad - detalleOC.CantidadRecibida;
                        if (dto.Cantidad > pendiente)
                            throw new ApplicationException($"No puede recibir {dto.Cantidad}. El saldo pendiente de la OC es {pendiente}.");

                        // Actualizar cantidad recibida en la OC
                        detalleOC.CantidadRecibida += dto.Cantidad;
                        _unitOfWork.Context.Update(detalleOC);

                        // Verificar si se completó la Orden de Compra
                        var oc = detalleOC.OrdenCompra;
                        var totalItemsOC = await _unitOfWork.Context.Set<DetalleOrdenCompra>()
                            .Where(d => d.IdOrdenCompra == oc.Id)
                            .ToListAsync();

                        oc.Estado = totalItemsOC.All(d => d.CantidadRecibida >= d.Cantidad) ? "Recibida Total" : "Recibida Parcial";
                        _unitOfWork.Context.Update(oc);
                    }
                }

                // C. Lógica de Costo Promedio Ponderado (CPP)
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

                    if (stockFinal > 0)
                        saldo.CostoPromedio = (costoActualTotal + costoNuevoTotal) / stockFinal;

                    saldo.StockActual = stockFinal;
                    saldo.FechaUltimoMovimiento = dto.FechaMovimiento;
                    await _unitOfWork.Inventario.UpdateAsync(saldo);
                }

                // D. Registrar el Movimiento físico
                var movimiento = _mapper.Map<MovimientoInventario>(dto);
                movimiento.TipoMovimiento = "INGRESO";
                movimiento.CostoUnitarioMovimiento = dto.CostoUnitarioCompra;
                movimiento.StockFinal = stockFinal;
                movimiento.Responsable = dto.ResponsableNombre;

                await _unitOfWork.MovimientosInventario.AddAsync(movimiento);

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error en RegistrarIngreso");
                throw;
            }
        }

        public async Task<bool> RegistrarSalida(MovimientoInventarioDto dto)
        {
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                // A. Validaciones y Stock
                var saldo = await _unitOfWork.Inventario.FindByKeysAsync(dto.MaterialId, dto.AlmacenId);
                if (saldo == null || saldo.StockActual < dto.Cantidad)
                    throw new ApplicationException($"Stock insuficiente. Actual: {saldo?.StockActual ?? 0}");

                // B. TRAZABILIDAD: Si es para atender un Requerimiento
                if (dto.DetalleRequerimientoId.HasValue)
                {
                    var detalleReq = await _unitOfWork.Context.Set<DetalleRequerimiento>()
                        .FirstOrDefaultAsync(d => d.Id == dto.DetalleRequerimientoId.Value);

                    if (detalleReq != null)
                    {
                        decimal pendienteReq = detalleReq.CantidadSolicitada - detalleReq.CantidadAtendida;
                        if (dto.Cantidad > pendienteReq)
                            throw new ApplicationException($"No puede despachar más de lo pendiente ({pendienteReq}).");

                        detalleReq.CantidadAtendida += dto.Cantidad;
                        _unitOfWork.Context.Update(detalleReq);
                    }
                }

                decimal costoUnitarioSalida = saldo.CostoPromedio;
                decimal nuevoStock = saldo.StockActual - dto.Cantidad;
                decimal costoTotalSalida = dto.Cantidad * costoUnitarioSalida;

                // C. Actualizar Stock e Inventario
                saldo.StockActual = nuevoStock;
                saldo.FechaUltimoMovimiento = dto.FechaMovimiento;
                await _unitOfWork.Inventario.UpdateAsync(saldo);

                // D. Registrar Movimiento
                var movimiento = _mapper.Map<MovimientoInventario>(dto);
                movimiento.TipoMovimiento = "SALIDA";
                movimiento.CostoUnitarioMovimiento = costoUnitarioSalida;
                movimiento.StockFinal = nuevoStock;
                movimiento.Responsable = dto.ResponsableNombre;
                await _unitOfWork.MovimientosInventario.AddAsync(movimiento);

                // E. Actualizar Costos de Proyecto y Partida (Tu lógica existente)
                if (dto.ProyectoId.HasValue && dto.Motivo == "CONSUMO_PROYECTO")
                {
                    var proyecto = await _unitOfWork.Proyectos.GetByIdAsync(dto.ProyectoId.Value);
                    if (proyecto != null)
                    {
                        proyecto.CostoEjecutado += costoTotalSalida;
                        await _unitOfWork.Proyectos.UpdateAsync(proyecto);
                    }

                    if (dto.PartidaId.HasValue && dto.PartidaId > 0)
                    {
                        var partida = await _unitOfWork.Context.Set<Partida>().FindAsync(dto.PartidaId.Value);
                        if (partida != null)
                        {
                            partida.CostoEjecutado += costoTotalSalida;
                            _unitOfWork.Context.Entry(partida).State = EntityState.Modified;
                        }
                    }
                }

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
        public async Task<IEnumerable<ConsumoMaterialProyectoDto>> GetConsumoDetalladoPorProyectoAsync(int proyectoId)
        {
            var movimientos = await _unitOfWork.Context.MovimientosInventario
                .Include(m => m.Material)
                .Where(m => m.ProyectoId == proyectoId && m.TipoMovimiento == "SALIDA")
                .ToListAsync();

            var totalInversion = movimientos.Sum(m => m.Cantidad * m.CostoUnitarioMovimiento);

            return movimientos
                .GroupBy(m => new { m.MaterialId, m.Material.Nombre, m.Material.UnidadMedida })
                .Select(g => new ConsumoMaterialProyectoDto
                {
                    MaterialNombre = g.Key.Nombre,
                    UnidadMedida = g.Key.UnidadMedida ?? "Und",
                    CantidadTotal = g.Sum(m => m.Cantidad),
                    CostoTotalAcumulado = g.Sum(m => m.Cantidad * m.CostoUnitarioMovimiento),
                    PorcentajeDelTotal = totalInversion > 0 ? (g.Sum(m => m.Cantidad * m.CostoUnitarioMovimiento) / totalInversion) * 100 : 0
                }).OrderByDescending(x => x.CostoTotalAcumulado).ToList();
        }
        public async Task<bool> RealizarTransferenciaAsync(TransferenciaDto dto)
        {
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                // 1. SALIDA DEL ORIGEN
                var inventarioOrigen = await _unitOfWork.Inventario.FindByKeysAsync(dto.MaterialId, dto.AlmacenOrigenId);
                if (inventarioOrigen == null || inventarioOrigen.StockActual < dto.Cantidad)
                    throw new ApplicationException("Stock insuficiente en el almacén de origen.");

                decimal costoPromedioOrigen = inventarioOrigen.CostoPromedio;
                inventarioOrigen.StockActual -= dto.Cantidad;
                await _unitOfWork.Inventario.UpdateAsync(inventarioOrigen);

                await _unitOfWork.MovimientosInventario.AddAsync(new MovimientoInventario
                {
                    AlmacenId = dto.AlmacenOrigenId,
                    MaterialId = dto.MaterialId,
                    TipoMovimiento = "SALIDA",
                    Motivo = "TRANSFERENCIA_SALIDA",
                    Cantidad = dto.Cantidad,
                    FechaMovimiento = DateTime.Now,
                    CostoUnitarioMovimiento = costoPromedioOrigen,
                    StockFinal = inventarioOrigen.StockActual,
                    Responsable = dto.ResponsableNombre,
                    Notas = $"Hacia Almacén ID: {dto.AlmacenDestinoId}. {dto.Observacion}"
                });

                // 2. INGRESO AL DESTINO
                var inventarioDestino = await _unitOfWork.Inventario.FindByKeysAsync(dto.MaterialId, dto.AlmacenDestinoId);
                if (inventarioDestino == null)
                {
                    // Si no existe, crear registro con costo del origen
                    await _unitOfWork.Inventario.AddAsync(new Inventario
                    {
                        MaterialId = dto.MaterialId,
                        AlmacenId = dto.AlmacenDestinoId,
                        StockActual = dto.Cantidad,
                        CostoPromedio = costoPromedioOrigen,
                        FechaUltimoMovimiento = DateTime.Now
                    });
                }
                else
                {
                    // Recalcular Costo Promedio Ponderado en destino
                    decimal costoTotalActual = inventarioDestino.StockActual * inventarioDestino.CostoPromedio;
                    decimal costoTransferencia = dto.Cantidad * costoPromedioOrigen;
                    inventarioDestino.StockActual += dto.Cantidad;
                    inventarioDestino.CostoPromedio = (costoTotalActual + costoTransferencia) / inventarioDestino.StockActual;
                    await _unitOfWork.Inventario.UpdateAsync(inventarioDestino);
                }

                await _unitOfWork.MovimientosInventario.AddAsync(new MovimientoInventario
                {
                    AlmacenId = dto.AlmacenDestinoId,
                    MaterialId = dto.MaterialId,
                    TipoMovimiento = "INGRESO",
                    Motivo = "TRANSFERENCIA_INGRESO",
                    Cantidad = dto.Cantidad,
                    FechaMovimiento = DateTime.Now,
                    CostoUnitarioMovimiento = costoPromedioOrigen,
                    StockFinal = (inventarioDestino?.StockActual ?? 0) + dto.Cantidad,
                    Responsable = dto.ResponsableNombre,
                    Notas = $"Desde Almacén ID: {dto.AlmacenOrigenId}. {dto.Observacion}"
                });

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}