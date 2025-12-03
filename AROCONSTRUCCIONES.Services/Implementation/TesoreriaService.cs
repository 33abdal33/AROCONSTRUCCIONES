using AROCONSTRUCCIONES.Dtos;
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
        private readonly IPdfService _pdfService;

        public TesoreriaService(IUnitOfWork unitOfWork, ILogger<TesoreriaService> logger, IPdfService pdfService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _pdfService = pdfService;
        }

        public async Task<bool> GenerarSolicitudPagoDesdeOC(int ordenCompraId, string userId)
        {
            try
            {
                // 1. Obtener la OC
                var oc = await _unitOfWork.OrdenesCompra.GetByIdWithDetailsAsync(ordenCompraId);

                if (oc == null) throw new Exception("Orden de Compra no encontrada.");
                if (oc.Estado == "Pendiente" || oc.Estado == "Cancelado")
                {
                    throw new ApplicationException($"No se puede generar pago. La OC está en estado '{oc.Estado}'.");
                }
                if (!oc.ProyectoId.HasValue)
                {
                    throw new ApplicationException("La OC no tiene un proyecto vinculado.");
                }

                // 2. Verificar duplicados
                var existeSP = await _unitOfWork.Context.Set<SolicitudPago>()
                    .AnyAsync(sp => sp.OrdenCompraId == ordenCompraId && sp.Estado != "Anulado");
                if (existeSP) return false;

                // --- 3. GENERACIÓN DE CORRELATIVO (SOLUCIÓN AL PROBLEMA) ---
                // Contamos cuántas SP existen para calcular el siguiente número
                // Ej: Si hay 5, el nuevo será 6 -> "SP-0006"
                int cantidadActual = await _unitOfWork.Context.Set<SolicitudPago>().CountAsync();
                int siguienteNumero = cantidadActual + 1;
                string codigoCorrelativo = $"SP-{siguienteNumero:D4}"; // D4 = Rellena con ceros (0001, 0015, etc.)

                // Validación de seguridad para NumeroDocumento (OC)
                // Si el código de la OC es muy largo (ej. por el error de duplicado REQ-REQ), lo cortamos a 20 chars.
                string numeroDocCorto = oc.Codigo.Length > 20
                    ? oc.Codigo.Substring(0, 20)
                    : oc.Codigo;

                // -----------------------------------------------------------

                var nuevaSP = new SolicitudPago
                {
                    Codigo = codigoCorrelativo, // Usamos el nuevo código corto
                    FechaSolicitud = DateTime.Now,
                    Estado = "Pendiente",
                    Moneda = "NUEVOS SOLES",
                    OrdenCompraId = oc.Id,
                    ProyectoId = oc.ProyectoId.Value,
                    ProveedorId = oc.IdProveedor,
                    SolicitadoPorUserId = userId,
                    MontoTotal = oc.Total,
                    MontoNetoAPagar = oc.Total,
                    Concepto = $"Pago OC {numeroDocCorto}", // Concepto más corto

                    // Snapshot de datos
                    BeneficiarioNombre = oc.Proveedor.RazonSocial,
                    BeneficiarioRUC = oc.Proveedor.RUC,
                    Banco = oc.Proveedor.Banco,
                    NumeroCuenta = oc.Proveedor.NumeroCuenta,
                    CCI = oc.Proveedor.CCI
                };

                var detalleSP = new DetalleSolicitudPago
                {
                    SolicitudPago = nuevaSP,
                    TipoDocumento = "OC",
                    SerieDocumento = "GEN",
                    NumeroDocumento = numeroDocCorto, // ¡AQUÍ USAMOS EL CÓDIGO SEGURO!
                    FechaEmisionDocumento = oc.FechaEmision,
                    Monto = oc.Total,
                    OrdenCompraId = oc.Id,
                    Observacion = "Generado desde Logística"
                };

                await _unitOfWork.Context.Set<SolicitudPago>().AddAsync(nuevaSP);
                await _unitOfWork.Context.Set<DetalleSolicitudPago>().AddAsync(detalleSP);
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
        public async Task RegistrarPagoAsync(PagarSolicitudDto dto, string userId)
        {
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                // 1. Obtener la Solicitud
                var solicitud = await _unitOfWork.Context.Set<SolicitudPago>()
                    .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

                if (solicitud == null) throw new Exception("Solicitud no encontrada");
                if (solicitud.Estado == "Pagado") throw new Exception("Esta solicitud ya fue pagada.");

                // 2. Obtener la Cuenta Bancaria (y bloquearla para concurrencia si fuera necesario)
                var cuenta = await _unitOfWork.Context.CuentasBancarias.FindAsync(dto.CuentaBancariaId);
                if (cuenta == null) throw new Exception("Cuenta bancaria no encontrada.");

                // 3. Validar Saldo (Opcional: permitir negativos si es cuenta corriente con crédito)
                if (cuenta.SaldoActual < solicitud.MontoNetoAPagar)
                {
                    throw new Exception($"Saldo insuficiente en {cuenta.BancoNombre}. Saldo: {cuenta.SaldoActual:N2}, Requerido: {solicitud.MontoNetoAPagar:N2}");
                }

                // 4. Descontar Saldo
                cuenta.SaldoActual -= solicitud.MontoNetoAPagar;

                // 5. Registrar Movimiento Bancario
                var movimiento = new MovimientoBancario
                {
                    CuentaBancariaId = cuenta.Id,
                    FechaMovimiento = dto.FechaPago,
                    TipoMovimiento = "EGRESO",
                    Monto = solicitud.MontoNetoAPagar,
                    SaldoDespues = cuenta.SaldoActual,
                    Descripcion = $"PAGO SP-{solicitud.Codigo} - {solicitud.BeneficiarioNombre}",
                    NumeroOperacion = dto.NumeroOperacion,
                    SolicitudPagoId = solicitud.Id
                };
                await _unitOfWork.Context.MovimientosBancarios.AddAsync(movimiento);

                // 6. Actualizar Solicitud
                solicitud.Estado = "Pagado";
                solicitud.FechaPago = dto.FechaPago;
                solicitud.AutorizadoPorUserId = userId;
                solicitud.Concepto += $" || PAGADO CON {cuenta.BancoNombre} OP: {dto.NumeroOperacion}";
                solicitud.Banco = cuenta.BancoNombre; // Guardamos histórico de con qué se pagó

                // Guardar todo
                await _unitOfWork.Context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<string> ObtenerUrlPdfSolicitud(int id)
        {
            var solicitud = await GetSolicitudByIdAsync(id);
            if (solicitud == null) throw new Exception("Solicitud no encontrada");

            // Llama a tu servicio PDF existente
            return await _pdfService.GenerarPdfSolicitudPago(solicitud);
        }
    }

}