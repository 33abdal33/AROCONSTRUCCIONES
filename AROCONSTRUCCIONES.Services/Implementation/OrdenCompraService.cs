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
            _logger.LogInformation("--- Iniciando CreateOrdenCompraAsync (Versión Robusta con Validación de Proveedor) ---");

            // 1. Validaciones iniciales
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

                // B. Datos de Cabecera
                ordenCompra.Estado = "Pendiente";
                ordenCompra.FechaEmision = DateTime.Now;
                ordenCompra.Moneda = dto.Moneda ?? "PEN";

                // Generar código (Considera usar una secuencia real en producción)
                string correlativo = (new Random().Next(1000, 9999)).ToString();
                ordenCompra.Codigo = $"OC-{DateTime.Now.Year}-{DateTime.Now.Month:00}-{correlativo}";

                decimal subTotalAcumulado = 0;

                foreach (var detalle in ordenCompra.Detalles)
                {
                    // --- NUEVA VALIDACIÓN DE ROBUSTEZ: ¿El proveedor vende este material? ---
                    // Verificamos en la tabla intermedia ProveedorMaterial
                    var vinculacionExiste = await _unitOfWork.Context.Set<ProveedorMaterial>()
                        .AnyAsync(pm => pm.ProveedorId == ordenCompra.IdProveedor &&
                                        pm.MaterialId == detalle.IdMaterial);

                    if (!vinculacionExiste)
                    {
                        var material = await _unitOfWork.Materiales.GetByIdAsync(detalle.IdMaterial);
                        throw new ApplicationException($"Inconsistencia: El proveedor '{proveedor.RazonSocial}' no distribuye el material '{material?.Nombre}'.");
                    }

                    // --- Cálculos de línea ---
                    decimal bruto = detalle.Cantidad * detalle.PrecioUnitario;
                    decimal descuento = bruto * (detalle.PorcentajeDescuento / 100);
                    detalle.ImporteTotal = bruto - descuento;

                    subTotalAcumulado += detalle.ImporteTotal;

                    // --- TRAZABILIDAD: Actualizar Requerimiento ---
                    if (detalle.IdDetalleRequerimiento.HasValue && detalle.IdDetalleRequerimiento > 0)
                    {
                        var reqDetalle = await _unitOfWork.DetalleRequerimiento
                                                          .GetByIdAsync(detalle.IdDetalleRequerimiento.Value);

                        if (reqDetalle != null)
                        {
                            reqDetalle.CantidadAtendida += detalle.Cantidad;
                            _unitOfWork.DetalleRequerimiento.Update(reqDetalle);
                        }
                    }
                }

                // D. Totales Finales
                ordenCompra.SubTotal = subTotalAcumulado;
                ordenCompra.Impuesto = subTotalAcumulado * 0.18m; // IGV 18%
                ordenCompra.Total = ordenCompra.SubTotal + ordenCompra.Impuesto;

                // E. Guardar OC (COMMIT 1)
                await _unitOfWork.OrdenesCompra.AddAsync(ordenCompra);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"OC Guardada con ID: {ordenCompra.Id}. Iniciando generación de PDF...");

                // F. Generar PDF
                var ocCompleta = await _unitOfWork.OrdenesCompra.GetByIdWithDetailsAsync(ordenCompra.Id);

                // Sincronizar totales para el PDF
                ocCompleta.SubTotal = ordenCompra.SubTotal;
                ocCompleta.Impuesto = ordenCompra.Impuesto;
                ocCompleta.Total = ordenCompra.Total;

                string rutaPdf = await _pdfService.GenerarPdfOrdenCompra(ocCompleta);
                ordenCompra.RutaPdf = rutaPdf;

                // G. Guardar Ruta PDF (COMMIT 2)
                await _unitOfWork.SaveChangesAsync();

                // H. Commit Final de la Transacción
                await transaction.CommitAsync();

                return ordenCompra;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR DENTRO de la transacción CreateOrdenCompraAsync. Haciendo Rollback.");
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<OrdenCompra> GetByIdWithDetailsAsync(int id)
        {
            return await _unitOfWork.Context.OrdenesCompra
                .Include(o => o.Proveedor)
                .Include(o => o.Proyecto)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Material)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}