using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    [Authorize(Roles = "Administrador,Usuario")]
    public class ReportesLogisticaController : Controller
    {
        private readonly IMaterialServices _materialService;
        private readonly IAlmacenService _almacenService;
        private readonly IMovimientoInventarioServices _movimientoService;
        private readonly IInventarioService _inventarioService;

        public ReportesLogisticaController(
            IMaterialServices materialService,
            IAlmacenService almacenService,
            IMovimientoInventarioServices movimientoService,
            IInventarioService inventarioService)
        {
            _materialService = materialService;
            _almacenService = almacenService;
            _movimientoService = movimientoService;
            _inventarioService = inventarioService;
        }

        // --- 1. ACCIÓN QUE CARGA LA PESTAÑA "REPORTES" ---
        // Tu _LogisticaTabPartial.cshtml llama a esta acción
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Cargamos los dropdowns para los filtros del reporte
            await CargarFiltrosViewBag();
            return PartialView("_ReportesIndexPartial");
        }

        // --- 2. ACCIÓN AJAX QUE GENERA EL REPORTE DE KÁRDEX ---
        [HttpGet]
        public async Task<IActionResult> GenerarReporteKardex(int materialId, int almacenId)
        {
            if (materialId == 0 || almacenId == 0)
                return BadRequest("Debe seleccionar un material y un almacén.");

            // 1. Obtener el Kárdex (el historial)
            var historial = await _movimientoService.GetHistorialPorMaterialYAlmacenAsync(materialId, almacenId);

            // 2. Obtener el Saldo (para el encabezado)
            var saldo = await _inventarioService.GetStockByKeysAsync(materialId, almacenId);
            ViewBag.Saldo = saldo; // Pasamos el saldo al ViewBag

            // 3. Devolvemos una NUEVA vista parcial solo con la tabla de resultados
            return PartialView("_KardexResultPartial", historial);
        }

        // (Aquí puedes añadir más acciones para otros reportes, como "GenerarReporteStockValorizado")

        // --- HELPER ---
        private async Task CargarFiltrosViewBag()
        {
            var materiales = await _materialService.GetAllActiveAsync();
            ViewBag.Materiales = new SelectList(materiales, "Id", "Nombre");

            var almacenes = await _almacenService.GetAllActiveAsync();
            ViewBag.Almacenes = new SelectList(almacenes, "Id", "Nombre");
        }
    }
}