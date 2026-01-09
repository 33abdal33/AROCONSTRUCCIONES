using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Repository.Interfaces;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    [Authorize(Roles = "Administrador,Usuario,Almacenero")]
    public class InventarioController : Controller
    {
        private readonly IInventarioService _inventarioService;
        private readonly IMovimientoInventarioServices _movimientoInventarioServices;
        private readonly IMaterialServices _materialService;
        private readonly IAlmacenService _almacenService;
        private readonly ILogisticaDashboardService _dashboardService;
        private readonly IProyectoService _proyectoService;
        private readonly IRequerimientoService _requerimientoService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InventarioController> _logger;

        public InventarioController(
            IInventarioService inventarioService,
            IMovimientoInventarioServices movimientoInventarioServices,
            IMaterialServices materialService,
            IAlmacenService almacenService,
            ILogisticaDashboardService dashboardService,
            IProyectoService proyectoService,
            IRequerimientoService requerimientoService,
            IUnitOfWork unitOfWork,
            ILogger<InventarioController> logger)
        {
            _inventarioService = inventarioService;
            _movimientoInventarioServices = movimientoInventarioServices;
            _materialService = materialService;
            _almacenService = almacenService;
            _dashboardService = dashboardService;
            _proyectoService = proyectoService;
            _requerimientoService = requerimientoService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Cargando Index de Inventario/Logística");
                ViewBag.DashboardData = await _dashboardService.GetSummaryAsync();
                await LoadModalSelectLists();
                ViewBag.OpenTab = TempData["OpenTab"];
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el Index de Inventario");
                TempData["ErrorMessage"] = $"Error al cargar el módulo: {ex.Message}";
                return View();
            }
        }
        // --- 📥 CARGA DE TABLA DE STOCK (CON FILTRO) ---
        [HttpGet]
        public async Task<IActionResult> InventarioPartial(int? proyectoId)
        {
            // 1. Cargamos los datos del inventario
            var saldos = await _inventarioService.GetAllStockViewAsync();

            // 2. Aplicamos el filtro si existe
            if (proyectoId.HasValue && proyectoId > 0)
            {
                saldos = saldos.Where(s => s.ProyectoId == proyectoId.Value).ToList();
            }

            // 3. ¡ESTA ES LA PARTE QUE FALTABA! 
            // Cargamos los selectores para que la vista parcial no falle al renderizar el dropdown
            await LoadModalSelectLists();

            return PartialView("_InventarioTablePartial", saldos);
        }

        // --- 📦 GESTIÓN DE DESPACHOS (ALMACÉN) ---

        [HttpGet]
        public async Task<IActionResult> GetDespachoRequerimientoModal(int id)
        {
            _logger.LogInformation($"Cargando modal de despacho para Requerimiento ID: {id}");

            var requerimiento = await _unitOfWork.Context.Requerimientos
                .Include(r => r.Proyecto)
                .Include(r => r.Detalles)
                    .ThenInclude(d => d.Material)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (requerimiento == null) return NotFound();

            await LoadModalSelectLists();
            return PartialView("_DespachoRequerimientoModalPartial", requerimiento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarDespacho(int RequerimientoId, int AlmacenId, string NroFacturaGuia, List<ItemDespachoDto> items)
        {
            // Iniciamos una transacción para que si falla el stock, se deshaga el cambio en el REQ
            using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
            try
            {
                if (items == null || !items.Any(i => i.CantidadADespachar > 0))
                    return Json(new { success = false, message = "Ingrese cantidades válidas." });

                foreach (var item in items.Where(i => i.CantidadADespachar > 0))
                {
                    var movDto = new MovimientoInventarioDto
                    {
                        TipoMovimiento = "SALIDA",

                        Motivo = "CONSUMO_PROYECTO",

                        AlmacenId = AlmacenId,

                        MaterialId = item.MaterialId,

                        Cantidad = item.CantidadADespachar,

                        DetalleRequerimientoId = item.DetalleRequerimientoId,

                        NroFacturaGuia = NroFacturaGuia,

                        ProyectoId = (await _unitOfWork.Requerimientos.GetByIdAsync(RequerimientoId))?.IdProyecto,

                        FechaMovimiento = DateTime.Now,

                        ResponsableNombre = User.Identity?.Name ?? "Almacenero"
                    };

                    // Aquí el servicio DEBE lanzar una excepción si no hay stock
                    await _movimientoInventarioServices.RegistrarSalida(movDto);
                }

                // Si todo salió bien, guardamos definitivamente
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Despacho procesado correctamente." });
            }
            catch (Exception ex)
            {
                // SI HAY ERROR (como falta de stock), deshacemos todo lo que se cambió en el REQ
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // --- 📊 LISTADOS DE REQUERIMIENTOS POR DEPARTAMENTO ---

        [HttpGet]
        public async Task<IActionResult> ListaPendientesDespachoPartial()
        {
            _logger.LogInformation("[InventarioController] Cargando pedidos pendientes para ALMACÉN.");

            // En el futuro, aquí podrías filtrar solo requerimientos que tengan stock disponible
            var requerimientos = await _requerimientoService.GetAllAprobadosAsync();

            return PartialView("_ListaPendientesDespachoPartial", requerimientos);
        }

        [HttpGet]
        public async Task<IActionResult> ListaRequerimientosAprobadosPartial()
        {
            _logger.LogInformation("[InventarioController] Cargando pedidos pendientes para LOGÍSTICA (COMPRAS).");

            // En el futuro, aquí podrías filtrar solo requerimientos que NO tengan stock en almacén
            var requerimientos = await _requerimientoService.GetAllAprobadosAsync();

            return PartialView("_ListaRequerimientosAprobadosPartial", requerimientos);
        }

        // --- 🔍 HISTORIAL Y DETALLES ---

        [HttpGet]
        public async Task<IActionResult> Details(int materialId, int almacenId)
        {
            var historial = await _movimientoInventarioServices.GetHistorialPorMaterialYAlmacenAsync(materialId, almacenId);
            var saldo = await _inventarioService.GetStockByKeysAsync(materialId, almacenId);

            if (saldo == null) return NotFound();

            ViewData["Saldo"] = saldo;
            return View("Details", historial);
        }

        // --- 🛠️ HELPERS ---

        private async Task LoadModalSelectLists()
        {
            var materiales = await _materialService.GetAllActiveAsync();
            ViewBag.Materiales = materiales.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.Codigo} - {m.Nombre} ({m.UnidadMedida})"
            }).ToList();

            ViewBag.Almacenes = new SelectList(await _almacenService.GetAllActiveAsync(), "Id", "Nombre");

            var proyectos = await _proyectoService.GetAllAsync();
            ViewBag.Proyectos = new SelectList(
                proyectos.Where(p => p.Estado == "En Ejecución" || p.Estado == "Planificación"),
                "Id", "NombreProyecto");
        }
    }
}