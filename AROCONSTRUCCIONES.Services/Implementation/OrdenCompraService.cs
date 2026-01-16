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
            _logger.LogInformation("--- Iniciando CreateOrdenCompraAsync (Versión Corregida) ---");

            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(dto.IdProveedor);
            if (proveedor == null) throw new ApplicationException("Proveedor no encontrado.");

            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                var ordenCompra = _mapper.Map<OrdenCompra>(dto);
                ordenCompra.Estado = "Pendiente";
                ordenCompra.FechaEmision = DateTime.Now;

                // Generar código
                string correlativo = (new Random().Next(1000, 9999)).ToString();
                ordenCompra.Codigo = $"OC-{DateTime.Now.Year}-{DateTime.Now.Month:00}-{correlativo}";

                decimal subTotalAcumulado = 0;

                foreach (var detalle in ordenCompra.Detalles)
                {
                    // 1. Validar vinculación Proveedor-Material
                    var vinculacionExiste = await _unitOfWork.Context.Set<ProveedorMaterial>()
                        .AnyAsync(pm => pm.ProveedorId == ordenCompra.IdProveedor && pm.MaterialId == detalle.IdMaterial);

                    if (!vinculacionExiste)
                    {
                        var material = await _unitOfWork.Materiales.GetByIdAsync(detalle.IdMaterial);
                        throw new ApplicationException($"El proveedor no distribuye el material '{material?.Nombre}'.");
                    }

                    // 2. Cálculos de línea
                    detalle.ImporteTotal = (detalle.Cantidad * detalle.PrecioUnitario) * (1 - (detalle.PorcentajeDescuento / 100));
                    subTotalAcumulado += detalle.ImporteTotal;

                    // --- TRAZABILIDAD CORREGIDA ---
                    // NO SUMAMOS a CantidadAtendida aquí. 
                    // Solo verificamos que el ID del detalle del requerimiento sea válido.
                }

                ordenCompra.SubTotal = subTotalAcumulado;
                ordenCompra.Impuesto = subTotalAcumulado * 0.18m;
                ordenCompra.Total = ordenCompra.SubTotal + ordenCompra.Impuesto;

                await _unitOfWork.OrdenesCompra.AddAsync(ordenCompra);
                await _unitOfWork.SaveChangesAsync();

                // 3. CAMBIAR ESTADO DEL REQUERIMIENTO (Sin tocar cantidades)
                if (dto.RequerimientoId.HasValue)
                {
                    var req = await _unitOfWork.Context.Requerimientos.FindAsync(dto.RequerimientoId.Value);
                    if (req != null)
                    {
                        req.Estado = "Con Orden"; // Ahora logística sabe que ya se compró
                        _unitOfWork.Context.Update(req);
                    }
                }

                // 4. Generar PDF
                var ocCompleta = await _unitOfWork.OrdenesCompra.GetByIdWithDetailsAsync(ordenCompra.Id);
                string rutaPdf = await _pdfService.GenerarPdfOrdenCompra(ocCompleta);
                ordenCompra.RutaPdf = rutaPdf;

                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return ordenCompra;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear OC");
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