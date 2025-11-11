using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class RecepcionService : IRecepcionService
    {
        // Inyectamos TODOS los repositorios que necesitamos + DbContext para la transacción
        private readonly IOrdenCompraRepository _ordenCompraRepo;
        private readonly IInventarioRepository _inventarioRepo;
        private readonly IMovimientoInventarioRepository _movimientoRepo;
        private readonly IMaterialRepository _materialRepo;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public RecepcionService(
            IOrdenCompraRepository ordenCompraRepo,
            IInventarioRepository inventarioRepo,
            IMovimientoInventarioRepository movimientoRepo,
            IMaterialRepository materialRepo,
            ApplicationDbContext dbContext,
            IMapper mapper)
        {
            _ordenCompraRepo = ordenCompraRepo;
            _inventarioRepo = inventarioRepo;
            _movimientoRepo = movimientoRepo;
            _materialRepo = materialRepo;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task RegistrarRecepcionAsync(RecepcionMaestroDto dto)
        {
            // 1. Iniciar Transacción (Tu patrón)
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 2. Obtener la Orden de Compra (con sus detalles)
                var oc = await _ordenCompraRepo.GetByIdWithDetailsAsync(dto.OrdenCompraId);
                if (oc == null)
                    throw new ApplicationException("Orden de Compra no encontrada.");
                if (oc.Estado == "Completado" || oc.Estado == "Cancelado")
                    throw new ApplicationException($"La OC {oc.Codigo} ya está cerrada.");

                bool algoRecibido = false;

                // 3. Iterar sobre los materiales que el usuario está recibiendo
                foreach (var detalleDto in dto.Detalles)
                {
                    decimal cantidadARecibir = detalleDto.CantidadARecibir;
                    if (cantidadARecibir <= 0) continue; // Ignorar filas que no se recibieron

                    algoRecibido = true;
                    var detalleOC = oc.Detalles.FirstOrDefault(d => d.Id == detalleDto.DetalleOrdenCompraId);
                    if (detalleOC == null)
                        throw new ApplicationException("Detalle de OC no encontrado.");

                    // Validar que no se reciba más de lo pendiente
                    decimal pendiente = detalleOC.Cantidad - detalleOC.CantidadRecibida;
                    if (cantidadARecibir > pendiente)
                        throw new ApplicationException($"Intenta recibir {cantidadARecibir} de {detalleDto.MaterialNombre}, pero solo quedan {pendiente} pendientes.");

                    // --- 4. LÓGICA DE INGRESO (Copiada de tu MovimientoInventarioService) ---
                    var saldo = await _inventarioRepo.FindByKeysAsync(detalleOC.IdMaterial, dto.AlmacenDestinoId);
                    decimal stockFinal;

                    // El costo es el de la Orden de Compra
                    decimal costoIngreso = detalleOC.PrecioUnitario;

                    if (saldo == null)
                    {
                        stockFinal = cantidadARecibir;
                        saldo = new Inventario
                        {
                            MaterialId = detalleOC.IdMaterial,
                            AlmacenId = dto.AlmacenDestinoId,
                            StockActual = stockFinal,
                            StockMinimo = detalleOC.Material?.StockMinimo ?? 0,
                            CostoPromedio = costoIngreso, // El primer costo es el de la compra
                            FechaUltimoMovimiento = dto.FechaRecepcion ?? DateTime.Now
                        };
                        await _inventarioRepo.AddAsync(saldo);
                    }
                    else
                    {
                        // Recalcular Costo Promedio Ponderado (CUPM)
                        decimal costoActualTotal = saldo.StockActual * saldo.CostoPromedio;
                        decimal costoNuevoTotal = cantidadARecibir * costoIngreso;
                        stockFinal = saldo.StockActual + cantidadARecibir;

                        saldo.CostoPromedio = (stockFinal > 0) ? (costoActualTotal + costoNuevoTotal) / stockFinal : 0;
                        saldo.StockActual = stockFinal;
                        saldo.FechaUltimoMovimiento = dto.FechaRecepcion ?? DateTime.Now;
                        await _inventarioRepo.UpdateAsync(saldo);
                    }
                    // --- Fin Lógica de Inventario ---

                    // 5. Crear el Movimiento de Inventario
                    var movimiento = new MovimientoInventario
                    {
                        MaterialId = detalleOC.IdMaterial,
                        AlmacenId = dto.AlmacenDestinoId,
                        TipoMovimiento = "INGRESO",
                        Motivo = $"INGRESO OC: {oc.Codigo}",
                        Cantidad = cantidadARecibir,
                        FechaMovimiento = dto.FechaRecepcion ?? DateTime.Now,
                        Responsable = dto.ResponsableNombre,
                        NroFacturaGuia = dto.NroFacturaGuia,
                        PrecioUnitario = costoIngreso,
                        CostoUnitarioMovimiento = costoIngreso,
                        ProveedorId = oc.IdProveedor,
                        StockFinal = stockFinal // El stock final calculado
                    };
                    await _movimientoRepo.AddAsync(movimiento);

                    // 6. Actualizar el detalle de la OC
                    detalleOC.CantidadRecibida += cantidadARecibir;
                }

                if (!algoRecibido)
                    throw new ApplicationException("No se ingresó una cantidad para ningún material.");

                // 7. Actualizar el estado general de la OC
                decimal totalPendiente = oc.Detalles.Sum(d => d.Cantidad - d.CantidadRecibida);
                oc.Estado = (totalPendiente == 0) ? "Completado" : "Recibido Parcial";

                await _ordenCompraRepo.UpdateAsync(oc); // Marca la OC para actualizar

                // 8. ¡COMMIT! (Guardar todo en la BD)
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Relanzar la excepción para que el controlador la atrape
            }
        }
        public async Task<RecepcionMaestroDto?> GetDatosParaModalRecepcionAsync(int id)
        {
            // 1. Buscamos la OC con todos sus detalles
            //    (Usamos el repo que ya tenías inyectado)
            var oc = await _ordenCompraRepo.GetByIdWithDetailsAsync(id);
            if (oc == null)
                return null;

            // 2. Mapeamos la Entidad al DTO
            var dto = _mapper.Map<RecepcionMaestroDto>(oc);

            // 3. Pre-llenamos las cantidades a recibir
            //    (Esta es la lógica que el controlador ya no hace)
            foreach (var detalle in dto.Detalles)
            {
                // CantidadPendiente se calcula automáticamente en el DTO
                detalle.CantidadARecibir = detalle.CantidadPendiente;
            }

            return dto;
        }
    }
}

