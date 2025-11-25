using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Services.Implementation
{
    public class TesoreriaService : ITesoreriaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TesoreriaService> _logger;

        public TesoreriaService(IUnitOfWork unitOfWork, ILogger<TesoreriaService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> GenerarSolicitudPagoDesdeOC(int ordenCompraId, string userId)
        {
            try
            {
                // 1. Obtener la OC con todos sus datos (Proveedor y Detalles)
                var oc = await _unitOfWork.OrdenesCompra.GetByIdWithDetailsAsync(ordenCompraId);

                if (oc == null) throw new Exception("Orden de Compra no encontrada.");
                if (!oc.ProyectoId.HasValue) throw new Exception("La OC no tiene un proyecto asignado, no se puede generar SP.");

                // 2. Verificar si ya existe una SP para esta OC (Evitar duplicados)
                var existeSP = await _unitOfWork.Context.Set<SolicitudPago>()
                    .AnyAsync(sp => sp.OrdenCompraId == ordenCompraId && sp.Estado != "Anulado");

                if (existeSP) return false; // Ya se generó

                // 3. Crear la Cabecera de la SP
                var nuevaSP = new SolicitudPago
                {
                    Codigo = $"SP-{DateTime.Now.Year}-{oc.Codigo}", // Ej: SP-2025-OC-001
                    FechaSolicitud = DateTime.Now,
                    Estado = "Pendiente",
                    Moneda = "NUEVOS SOLES", // <-- AÑADIR ESTA LÍNEA

                    // Vinculaciones
                    OrdenCompraId = oc.Id,
                    ProyectoId = oc.ProyectoId.Value, // ¡Aquí usamos el dato que añadimos!
                    ProveedorId = oc.IdProveedor,
                    SolicitadoPorUserId = userId,

                    // Datos Financieros (Copia fiel de la OC)
                    MontoTotal = oc.Total,
                    Concepto = $"Pago por OC {oc.Codigo} - {oc.Observaciones}",

                    // Datos del Beneficiario (Instantánea del momento)
                    BeneficiarioNombre = oc.Proveedor.RazonSocial,
                    BeneficiarioRUC = oc.Proveedor.RUC,
                    Banco = oc.Proveedor.Banco,
                    NumeroCuenta = oc.Proveedor.NumeroCuenta,
                    CCI = oc.Proveedor.CCI
                };

                // 4. Crear el Detalle de la SP (La línea que referencia a la OC)
                var detalleSP = new DetalleSolicitudPago
                {
                    SolicitudPago = nuevaSP, // EF Core enlazará esto automáticamente
                    TipoDocumento = "OC",
                    SerieDocumento = "GEN",
                    NumeroDocumento = oc.Codigo,
                    FechaEmisionDocumento = oc.FechaEmision,
                    Monto = oc.Total,
                    OrdenCompraId = oc.Id,
                    Observacion = "Generado automáticamente desde Logística"
                };

                // 5. Guardar en Base de Datos
                await _unitOfWork.Context.Set<SolicitudPago>().AddAsync(nuevaSP);
                await _unitOfWork.Context.Set<DetalleSolicitudPago>().AddAsync(detalleSP);

                // Opcional: Actualizar estado de OC para indicar que está en proceso de pago
                // oc.Estado = "En Proceso de Pago"; 

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Solicitud de Pago {nuevaSP.Codigo} generada exitosamente.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar SP desde OC");
                throw;
            }
        }
        public async Task<IEnumerable<SolicitudPago>> GetAllSolicitudesAsync()
        {
            return await _unitOfWork.Context.Set<SolicitudPago>()
                .Include(sp => sp.Proveedor)
                .Include(sp => sp.Proyecto) // ¡Importante para mostrar el nombre del proyecto!
                .OrderByDescending(sp => sp.FechaSolicitud) // Las más recientes primero
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SolicitudPago?> GetSolicitudByIdAsync(int id)
        {
            return await _unitOfWork.Context.Set<SolicitudPago>()
               .Include(sp => sp.Proveedor)
               .Include(sp => sp.Proyecto)
               .Include(sp => sp.OrdenCompra)
               .Include(sp => sp.Detalles) // Para ver qué facturas están adjuntas
               .FirstOrDefaultAsync(sp => sp.Id == id);
        }
    }
}