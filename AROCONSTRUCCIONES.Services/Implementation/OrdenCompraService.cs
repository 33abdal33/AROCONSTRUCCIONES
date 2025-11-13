using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using AutoMapper;
using Microsoft.Extensions.Logging; // <-- 1. AÑADIR
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
        private readonly ILogger<OrdenCompraService> _logger; // <-- 2. AÑADIR

        public OrdenCompraService(
            IUnitOfWork unitOfWork,
            IPdfService pdfService,
            IMapper mapper,
            ILogger<OrdenCompraService> logger) // <-- 3. AÑADIR
        {
            _unitOfWork = unitOfWork;
            _pdfService = pdfService;
            _mapper = mapper;
            _logger = logger; // <-- 4. AÑADIR
        }

        public async Task<IEnumerable<OrdenCompraListDto>> GetAllOrdenesCompraAsync()
        {
            // ... (Este método no cambia)
            var ordenes = await _unitOfWork.OrdenesCompra.GetAllWithProveedorAsync();
            return _mapper.Map<IEnumerable<OrdenCompraListDto>>(ordenes);
        }

        public async Task<OrdenCompra> CreateOrdenCompraAsync(OrdenCompraCreateDto dto)
        {
            _logger.LogInformation("--- Iniciando CreateOrdenCompraAsync ---"); // LOG

            // 1. Validaciones
            _logger.LogInformation("Validando proveedor...");
            var proveedor = await _unitOfWork.Proveedores.GetByIdAsync(dto.IdProveedor);
            if (proveedor == null)
                throw new ApplicationException($"El Proveedor con ID {dto.IdProveedor} no fue encontrado.");

            if (dto.Detalles == null || !dto.Detalles.Any())
                throw new ApplicationException("La Orden de Compra debe tener al menos un material.");

            _logger.LogInformation("Validación completada. Iniciando transacción.");
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                var ordenCompra = _mapper.Map<OrdenCompra>(dto);
                ordenCompra.Estado = "Pendiente";
                ordenCompra.FechaEmision = DateTime.Now;
                ordenCompra.Total = ordenCompra.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

                _logger.LogInformation("Entidad mapeada. Agregando al repositorio.");
                await _unitOfWork.OrdenesCompra.AddAsync(ordenCompra);

                _logger.LogInformation("Llamando a SaveChangesAsync (COMMIT 1 - Obtener ID)");
                await _unitOfWork.SaveChangesAsync(); // COMMIT 1
                _logger.LogInformation($"COMMIT 1 Exitoso. Nuevo ID de OC: {ordenCompra.Id}");


                // 5. Generar PDF
                _logger.LogInformation("Recargando OC completa para generar PDF...");
                var ocCompleta = await _unitOfWork.OrdenesCompra.GetByIdWithDetailsAsync(ordenCompra.Id);

                _logger.LogInformation("Llamando a PdfService.GenerarPdfOrdenCompra...");
                string rutaPdf = await _pdfService.GenerarPdfOrdenCompra(ocCompleta); // <-- PUNTO PROBABLE DE FALLO
                _logger.LogInformation($"PdfService Exitoso. Ruta: {rutaPdf}");


                ordenCompra.RutaPdf = rutaPdf;

                _logger.LogInformation("Llamando a SaveChangesAsync (COMMIT 2 - Guardar Ruta PDF)");
                await _unitOfWork.SaveChangesAsync(); // COMMIT 2
                _logger.LogInformation("COMMIT 2 Exitoso.");

                // 7. Confirmar
                _logger.LogInformation("Confirmando transacción (CommitAsync)...");
                await transaction.CommitAsync();
                _logger.LogInformation("--- Transacción COMPLETA ---");

                return ordenCompra;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR DENTRO de la transacción. Haciendo Rollback."); // LOG DE ERROR
                await transaction.RollbackAsync();
                throw new ApplicationException("Error al crear la orden de compra y el pdf: " + ex.Message, ex);
            }
        }
    }
}