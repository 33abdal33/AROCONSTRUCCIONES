using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IProyectoService _proyectoService;
        private readonly IExportService _exportService; // <--- NUEVA INYECCIÓN

        public ReportesLogisticaController(
            IMaterialServices materialService,
            IAlmacenService almacenService,
            IMovimientoInventarioServices movimientoService,
            IInventarioService inventarioService,
            IProyectoService proyectoService,
            IExportService exportService) // <--- NUEVA INYECCIÓN
        {
            _materialService = materialService;
            _almacenService = almacenService;
            _movimientoService = movimientoService;
            _inventarioService = inventarioService;
            _proyectoService = proyectoService;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await CargarFiltrosViewBag();
            return PartialView("_ReportesIndexPartial");
        }

        // --- EXPORTACIÓN A EXCEL (USANDO EL SERVICIO) ---

        [HttpGet]
        public async Task<IActionResult> ExportarStockExcel(int? almacenId)
        {
            var stock = await _inventarioService.GetAllStockViewAsync();
            if (almacenId.HasValue && almacenId > 0)
            {
                stock = stock.Where(s => s.AlmacenId == almacenId.Value);
            }

            var fileContent = _exportService.GenerarExcelStock(stock);
            string fileName = $"Stock_Valorizado_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportarConsumoExcel(int proyectoId)
        {
            if (proyectoId == 0) return BadRequest();

            var reporte = await _movimientoService.GetConsumoDetalladoPorProyectoAsync(proyectoId);
            var proyecto = await _proyectoService.GetByIdAsync(proyectoId);

            var fileContent = _exportService.GenerarExcelConsumo(reporte, proyecto?.NombreProyecto ?? "Proyecto");
            string fileName = $"Consumo_Obra_{DateTime.Now:yyyyMMdd}.xlsx";

            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // --- REPORTES VISUALES (MANTENEMOS TUS ACCIONES) ---

        [HttpGet]
        public async Task<IActionResult> GenerarReporteKardex(int materialId, int almacenId)
        {
            if (materialId == 0 || almacenId == 0) return BadRequest("Seleccione filtros.");
            var historial = await _movimientoService.GetHistorialPorMaterialYAlmacenAsync(materialId, almacenId);
            var saldo = await _inventarioService.GetStockByKeysAsync(materialId, almacenId);
            ViewBag.Saldo = saldo;
            return PartialView("_KardexResultPartial", historial);
        }

        [HttpGet]
        public async Task<IActionResult> GenerarReporteConsumoProyecto(int proyectoId)
        {
            if (proyectoId == 0) return BadRequest();
            ViewBag.ProyectoId = proyectoId; // <--- AGREGAR ESTO
            var reporte = await _movimientoService.GetConsumoDetalladoPorProyectoAsync(proyectoId);
            return PartialView("_ConsumoProyectoResultPartial", reporte);
        }

        [HttpGet]
        public async Task<IActionResult> GenerarReporteStockValorizado(int? almacenId)
        {
            ViewBag.AlmacenId = almacenId; // <--- AGREGAR ESTO
            var stock = await _inventarioService.GetAllStockViewAsync();
            if (almacenId.HasValue && almacenId > 0) stock = stock.Where(s => s.AlmacenId == almacenId.Value);
            return PartialView("_StockValorizadoResultPartial", stock);
        }

        private async Task CargarFiltrosViewBag()
        {
            ViewBag.Materiales = new SelectList(await _materialService.GetAllActiveAsync(), "Id", "Nombre");
            ViewBag.Almacenes = new SelectList(await _almacenService.GetAllActiveAsync(), "Id", "Nombre");
            ViewBag.Proyectos = new SelectList(await _proyectoService.GetAllAsync(), "Id", "NombreProyecto");
        }
    }
}