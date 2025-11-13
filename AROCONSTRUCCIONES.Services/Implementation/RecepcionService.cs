using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
// using AROCONSTRUCCIONES.Persistence; // <-- SE VA
using AROCONSTRUCCIONES.Repository.Interfaces; // <-- AÑADIDO
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class RecepcionService : IRecepcionService
    {
        private readonly IUnitOfWork _unitOfWork; // <-- CAMBIO
        private readonly IMapper _mapper;

        public RecepcionService(
            IUnitOfWork unitOfWork, // <-- CAMBIO
            IMapper mapper)
        {
            _unitOfWork = unitOfWork; // <-- CAMBIO
            _mapper = mapper;
        }

        public async Task<RecepcionMaestroDto?> GetDatosParaModalRecepcionAsync(int id)
        {
            // 1. Buscamos la OC usando el UoW
            var oc = await _unitOfWork.OrdenesCompra.GetByIdWithDetailsAsync(id);
            if (oc == null)
                return null;

            // 2. Mapeamos (sin cambios aquí)
            var dto = _mapper.Map<RecepcionMaestroDto>(oc);
            foreach (var detalle in dto.Detalles)
            {
                detalle.CantidadARecibir = detalle.CantidadPendiente;
            }
            return dto;
        }

        public async Task RegistrarRecepcionAsync(RecepcionMaestroDto dto)
        {
            // 1. Transacción NO es necesaria manualmente.
            // SaveChangesAsync() al final será atómico.
            try
            {
                // 2. Obtener la OC (usando UoW)
                var oc = await _unitOfWork.OrdenesCompra.GetByIdWithDetailsAsync(dto.OrdenCompraId);
                if (oc == null)
                    throw new ApplicationException("Orden de Compra no encontrada.");
                if (oc.Estado == "Completado" || oc.Estado == "Cancelado")
                    throw new ApplicationException($"La OC {oc.Codigo} ya está cerrada.");

                bool algoRecibido = false;

                // 3. Iterar sobre los materiales recibidos
                foreach (var detalleDto in dto.Detalles)
                {
                    decimal cantidadARecibir = detalleDto.CantidadARecibir;
                    if (cantidadARecibir <= 0) continue;

                    algoRecibido = true;
                    var detalleOC = oc.Detalles.FirstOrDefault(d => d.Id == detalleDto.DetalleOrdenCompraId);
                    if (detalleOC == null)
                        throw new ApplicationException("Detalle de OC no encontrado.");

                    decimal pendiente = detalleOC.Cantidad - detalleOC.CantidadRecibida;
                    if (cantidadARecibir > pendiente)
                        throw new ApplicationException($"Intenta recibir {cantidadARecibir} de {detalleDto.MaterialNombre}, pero solo quedan {pendiente} pendientes.");

                    // 4. Lógica de INVENTARIO (usando UoW)
                    var saldo = await _unitOfWork.Inventario.FindByKeysAsync(detalleOC.IdMaterial, dto.AlmacenDestinoId);
                    decimal stockFinal;
                    decimal costoIngreso = detalleOC.PrecioUnitario;
                    DateTime fechaRecepcion = dto.FechaRecepcion ?? DateTime.Now;

                    if (saldo == null)
                    {
                        stockFinal = cantidadARecibir;
                        saldo = new Inventario
                        {
                            MaterialId = detalleOC.IdMaterial,
                            AlmacenId = dto.AlmacenDestinoId,
                            StockActual = stockFinal,
                            StockMinimo = detalleOC.Material?.StockMinimo ?? 0,
                            CostoPromedio = costoIngreso,
                            FechaUltimoMovimiento = fechaRecepcion
                        };
                        await _unitOfWork.Inventario.AddAsync(saldo);
                    }
                    else
                    {
                        decimal costoActualTotal = saldo.StockActual * saldo.CostoPromedio;
                        decimal costoNuevoTotal = cantidadARecibir * costoIngreso;
                        stockFinal = saldo.StockActual + cantidadARecibir;
                        saldo.CostoPromedio = (stockFinal > 0) ? (costoActualTotal + costoNuevoTotal) / stockFinal : 0;
                        saldo.StockActual = stockFinal;
                        saldo.FechaUltimoMovimiento = fechaRecepcion;
                        await _unitOfWork.Inventario.UpdateAsync(saldo);
                    }

                    // 5. Crear el MOVIMIENTO (usando UoW)
                    var movimiento = new MovimientoInventario
                    {
                        MaterialId = detalleOC.IdMaterial,
                        AlmacenId = dto.AlmacenDestinoId,
                        TipoMovimiento = "INGRESO",
                        Motivo = $"INGRESO OC: {oc.Codigo}",
                        Cantidad = cantidadARecibir,
                        FechaMovimiento = fechaRecepcion,
                        Responsable = dto.ResponsableNombre,
                        NroFacturaGuia = dto.NroFacturaGuia,
                        PrecioUnitario = costoIngreso,
                        CostoUnitarioMovimiento = costoIngreso,
                        ProveedorId = oc.IdProveedor,
                        StockFinal = stockFinal
                    };
                    await _unitOfWork.MovimientosInventario.AddAsync(movimiento);

                    // 6. Actualizar el detalle de la OC (EF Core rastrea 'oc')
                    detalleOC.CantidadRecibida += cantidadARecibir;
                }

                if (!algoRecibido)
                    throw new ApplicationException("No se ingresó una cantidad para ningún material.");

                // 7. Actualizar el estado general de la OC
                decimal totalPendiente = oc.Detalles.Sum(d => d.Cantidad - d.CantidadRecibida);
                oc.Estado = (totalPendiente == 0) ? "Completado" : "Recibido Parcial";
                await _unitOfWork.OrdenesCompra.UpdateAsync(oc);

                // 8. ¡COMMIT ATÓMICO!
                // Guarda todo: (Inventario + Movimiento + DetalleOC + OrdenCompra)
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                // No hay rollback manual, EF Core lo maneja.
                throw; // Relanzar la excepción
            }
        }
    }
}