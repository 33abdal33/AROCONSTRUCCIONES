using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace AROCONSTRUCCIONES.Controllers
{
    public class InventarioController : Controller
    {
        // 1. INYECCIÓN DE SERVICIOS (El tuyo estaba perfecto)
        private readonly IInventarioService _inventarioService;
        private readonly IMovimientoInventarioServices _movimientoInventarioServices;
        private readonly IMaterialServices _materialService;
        private readonly IAlmacenService _almacenService;
        private readonly IProveedorService _proveedorService;
        private readonly ILogisticaDashboardService _dashboardService;
        private readonly IProyectoService _proyectoService;

        public InventarioController(
            IInventarioService inventarioService,
            IMovimientoInventarioServices movimientoInventarioServices,
            IMaterialServices materialService,
            IAlmacenService almacenService,
            IProveedorService proveedorService,
            ILogisticaDashboardService dashboardService,
            IProyectoService proyectoService)
        {
            _inventarioService = inventarioService;
            _movimientoInventarioServices = movimientoInventarioServices;
            _materialService = materialService;
            _almacenService = almacenService;
            _proveedorService = proveedorService;
            _dashboardService = dashboardService;
            _proyectoService = proyectoService;
        }

        // --- 2. ACCIÓN INDEX (MODIFICADA) ---
        // Esta acción AHORA solo carga el "cascarón" o contenedor de la página.
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // 1. Cargar datos para el Header (Kardex, etc.)
                ViewBag.DashboardData = await _dashboardService.GetSummaryAsync();

                // 2. Cargar datos para los modales (los necesitamos listos)
                await LoadModalSelectLists();

                // 3. Cargar la pestaña de TempData
                ViewBag.OpenTab = TempData["OpenTab"];

                // 4. Devolvemos la vista SIN datos de tabla.
                // El JavaScript se encargará de llamar a "InventarioPartial"
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cargar el módulo: {ex.Message}";
                ViewBag.DashboardData = new LogisticaDashboardDto();
                return View();
            }
        }

        // --- 3. ¡¡ACCIÓN NUEVA!! (La que faltaba) ---
        // Esta es la acción que tu AJAX busca (la que daba 404).
        // Esta acción carga SOLAMENTE la TABLA de inventario.
        [HttpGet]
        public async Task<IActionResult> InventarioPartial()
        {
            try
            {
                // La lógica que antes estaba en Index() ahora está aquí
                var saldos = await _inventarioService.GetAllStockViewAsync();

                // Devuelve solo el archivo _InventarioTablePartial.cshtml
                return PartialView("_InventarioTablePartial", saldos);
            }
            catch (Exception)
            {
                // Si falla, devuelve un partial de error
                return PartialView("_ErrorAlCargarPartial"); // (Puedes crear un partial simple que diga "Error")
            }
        }

        // --- 4. ¡¡ACCIÓN NUEVA PARA EL MODAL!! ---
        // Esta es la acción que el botón "Nuevo Movimiento" llama.
        // Carga el formulario del modal.
        [HttpGet]
        public async Task<IActionResult> CargarFormularioMovimiento()
        {
            // Carga los dropdowns (Materiales, Almacenes, etc.)
            await LoadModalSelectLists();

            var model = new MovimientoInventarioDto
            {
                TipoMovimiento = "INGRESO" // Valor por defecto
            };

            // Apunta a la ruta completa del modal que te di
            return PartialView("~/Views/MovimientoInventario/_MovimientoUnificadoModalPartial.cshtml", model);
        }
        // --- 4. ACTUALIZAR ESTE MÉTODO ---
        private async Task LoadModalSelectLists()
        {
            // --- Cargar Materiales ---
            var materiales = await _materialService.GetAllActiveAsync();
            ViewBag.Materiales = materiales
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.Codigo} - {m.Nombre} ({m.UnidadMedida})"
                })
                .ToList();

            // --- Cargar Almacenes ---
            var almacenes = await _almacenService.GetAllActiveAsync();
            ViewBag.Almacenes = new SelectList(almacenes, "Id", "Nombre");

            // --- Cargar Proveedores ---
            var proveedores = await _proveedorService.GetAllActiveAsync();
            ViewBag.Proveedores = new SelectList(proveedores, "Id", "RazonSocial");

            // --- ¡NUEVO! Cargar Proyectos (solo activos) ---
            var proyectos = await _proyectoService.GetAllAsync();
            ViewBag.Proyectos = new SelectList(
                proyectos.Where(p => p.Estado == "En Ejecución" || p.Estado == "Planificación"),
                "Id",
                "NombreProyecto");
        }

        // --- 6. MÉTODO DETAILS (El tuyo estaba perfecto) ---
        [HttpGet]
        public async Task<IActionResult> Details(int materialId, int almacenId)
        {
            var historial = await _movimientoInventarioServices
                                  .GetHistorialPorMaterialYAlmacenAsync(materialId, almacenId);

            var saldo = await _inventarioService.GetStockByKeysAsync(materialId, almacenId);

            if (saldo == null) return NotFound();

            ViewData["Saldo"] = saldo;
            return View("Details", historial);
        }
    }
}