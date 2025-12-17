using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.EntityFrameworkCore; // Necesario para UnitOfWork si accedes directo a Context
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class OrdenCompraService : IOrdenCompraServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPdfService _pdfService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdenCompraService> _logger;

        public OrdenCompraService(
            IUnitOfWork unitOfWork,
            IPdfService pdfService,
            IMapper mapper,
            ILogger<OrdenCompraService> logger)
        {
            _unitOfWork = unitOfWork;
            _pdfService = pdfService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<OrdenCompraListDto>> GetAllOrdenesCompraAsync()
        {
            var ordenes = await _unitOfWork.OrdenesCompra.GetAllWithProveedorAsync();
            return _mapper.Map<IEnumerable<OrdenCompraListDto>>(ordenes);
        }

        public async Task<OrdenCompra> CreateOrdenCompraAsync(OrdenCompraCreateDto dto)
        {
            _logger.LogInformation("--- Iniciando CreateOrdenCompraAsync (Versión Robusta) ---");

            // 1. Validaciones
            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(dto.IdProveedor);
            if (proveedor == null)
                throw new ApplicationException($"El Proveedor con ID {dto.IdProveedor} no fue encontrado.");

            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new ApplicationException("La Orden de Compra debe tener al menos un material.");

            // 2. Transacción
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                // A. Mapeo Inicial
                var ordenCompra = _mapper.Map<OrdenCompra>(dto);

                // B. Datos de Cabecera (ROBUSTEZ)
                ordenCompra.Estado = "Pendiente";
                ordenCompra.FechaEmision = DateTime.Now;
                ordenCompra.Moneda = dto.Moneda ?? "PEN"; // Asume Soles si viene null

                // Generar código único (Lógica simple, mejorable con correlativos por año)
                string correlativo = (new Random().Next(1000, 9999)).ToString();
                ordenCompra.Codigo = $"OC-{DateTime.Now.Year}-{DateTime.Now.Month:00}-{correlativo}";

                // C. Procesar Detalles y Cálculos (ROBUSTEZ EXTREMA)
                decimal subTotalAcumulado = 0;

                // NOTA: Es importante iterar sobre la lista mapeada 'ordenCompra.Detalles' 
                // pero necesitamos acceder al DTO original para saber el 'IdDetalleRequerimiento'
                // si es que AutoMapper no lo hizo automáticamente.

                // Si AutoMapper ya pasó el IdDetalleRequerimiento, perfecto. Si no, habría que hacerlo manual.
                // Asumiremos que AutoMapper funcionó bien gracias al Profile.

                foreach (var detalle in ordenCompra.Detalles)
                {
                    // 1. Calcular Totales por línea (Sin recalcular en vista)
                    // (Cantidad * Precio) - Descuento
                    decimal bruto = detalle.Cantidad * detalle.PrecioUnitario;
                    decimal descuento = bruto * (detalle.PorcentajeDescuento / 100);
                    detalle.ImporteTotal = bruto - descuento;

                    subTotalAcumulado += detalle.ImporteTotal;

                    // 2. TRAZABILIDAD: Actualizar Requerimiento (EL HILO CONDUCTOR)
                    if (detalle.IdDetalleRequerimiento.HasValue && detalle.IdDetalleRequerimiento > 0)
                    {
                        var reqDetalle = await _unitOfWork.DetalleRequerimiento
                                                          .GetByIdAsync(detalle.IdDetalleRequerimiento.Value);

                        if (reqDetalle != null)
                        {
                            reqDetalle.CantidadAtendida += detalle.Cantidad;
                            _unitOfWork.DetalleRequerimiento.Update(reqDetalle);

                            // Opcional: Validar si se pasó de la cantidad solicitada
                            /* if(reqDetalle.CantidadAtendida > reqDetalle.CantidadSolicitada) { ... } */
                        }
                    }
                }

                // D. Totales Finales (ROBUSTEZ FINANCIERA)
                ordenCompra.SubTotal = subTotalAcumulado;
                ordenCompra.Impuesto = subTotalAcumulado * 0.18m; // IGV 18%
                ordenCompra.Total = ordenCompra.SubTotal + ordenCompra.Impuesto;

                // E. Guardar OC (COMMIT 1)
                await _unitOfWork.OrdenesCompra.AddAsync(ordenCompra);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"OC Guardada con ID: {ordenCompra.Id}. Iniciando generación de PDF...");

                // F. Generar PDF
                // Necesitamos recargar la entidad con sus relaciones (Proveedor, Detalles+Material) para el PDF
                var ocCompleta = await _unitOfWork.OrdenesCompra.GetByIdWithDetailsAsync(ordenCompra.Id);

                // Aseguramos que los totales estén presentes (a veces el reload no trae propiedades calculadas si no se guardaron bien)
                ocCompleta.SubTotal = ordenCompra.SubTotal;
                ocCompleta.Impuesto = ordenCompra.Impuesto;
                ocCompleta.Total = ordenCompra.Total;

                string rutaPdf = await _pdfService.GenerarPdfOrdenCompra(ocCompleta);
                ordenCompra.RutaPdf = rutaPdf;

                // G. Guardar Ruta PDF (COMMIT 2)
                await _unitOfWork.SaveChangesAsync();

                // H. Commit Final
                await transaction.CommitAsync();

                return ordenCompra;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR DENTRO de la transacción CreateOrdenCompraAsync. Rollback.");
                await transaction.RollbackAsync();
                throw; // Relanzar para que el controlador lo maneje
            }
        }
    }
}