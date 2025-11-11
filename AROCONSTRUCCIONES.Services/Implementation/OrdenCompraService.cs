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
    public class OrdenCompraService : IOrdenCompraServices
    {
        // Inyectamos todo lo que necesitamos, siguiendo tu patrón
        private readonly IOrdenCompraRepository _ordenCompraRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IMaterialRepository _materialRepository; // Para validación futura
        private readonly ApplicationDbContext _dbContext; // Para la transacción
        private readonly IPdfService _pdfService;
        private readonly IMapper _mapper;

        public OrdenCompraService(
            IOrdenCompraRepository ordenCompraRepository,
            IProveedorRepository proveedorRepository,
            IMaterialRepository materialRepository,
            ApplicationDbContext dbContext,
            IInventarioRepository inventarioRepository,
            IPdfService pdfService,
            IMapper mapper)
        {
            _ordenCompraRepository = ordenCompraRepository;
            _proveedorRepository = proveedorRepository;
            _materialRepository = materialRepository;
            _dbContext = dbContext;
            _pdfService = pdfService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrdenCompraListDto>> GetAllOrdenesCompraAsync()
        {
            var ordenes = await _ordenCompraRepository.GetAllWithProveedorAsync();
            return _mapper.Map<IEnumerable<OrdenCompraListDto>>(ordenes);
        }

        public async Task<OrdenCompra> CreateOrdenCompraAsync(OrdenCompraCreateDto dto)
        {
            // 1. Validaciones de negocio
            var proveedor = await _proveedorRepository.GetByIdAsync(dto.IdProveedor);
            if (proveedor == null)
            {
                throw new ApplicationException($"El Proveedor con ID {dto.IdProveedor} no fue encontrado.");
            }

            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                throw new ApplicationException("La Orden de Compra debe tener al menos un material en el detalle.");
            }

            // (Aquí podrías añadir una validación para asegurar que todos los IdMaterial existen)

            // 2. Iniciar Transacción (igual que en tu MovimientoInventarioService)
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 3. Mapear DTO a Entidad
                // Gracias al Profile, esto mapea OrdenCompra Y su lista ICollection<DetalleOrdenCompra>
                var ordenCompra = _mapper.Map<OrdenCompra>(dto);

                // 4. Lógica de negocio y valores por defecto
                ordenCompra.Estado = "Pendiente";
                ordenCompra.FechaEmision = DateTime.Now;

                // Calculamos el total basado en los detalles (ya mapeados)
                ordenCompra.Total = ordenCompra.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

                // 5. Agregar al Repositorio (SOLO EN MEMORIA)
                // EF Core es lo suficientemente inteligente para guardar la 'ordenCompra' (Padre)
                // y todos los 'Detalles' (Hijos) que están en su colección.
                await _ordenCompraRepository.AddAsync(ordenCompra);

                // 6. Guardar y completar la transacción
                // Este es el único SaveChanges(). Guarda la OC y los Detalles
                await _dbContext.SaveChangesAsync();
                var ocCompleta = await _ordenCompraRepository.GetByIdWithDetailsAsync(ordenCompra.Id);

                string rutaPdf = await _pdfService.GenerarPdfOrdenCompra(ocCompleta);
                ordenCompra.RutaPdf = rutaPdf;
                await _dbContext.SaveChangesAsync(); // ¡COMMIT 2! (Guarda la ruta del PDF)



                await transaction.CommitAsync();

                return ordenCompra; // Devolvemos la entidad creada
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Relanzamos la excepción para que el Controlador la atrape
                throw new ApplicationException("Error al crear la orden de compra y el pdf: " + ex.Message, ex);
            }
        }

    }
}
