using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    public class ProyectoController : Controller
    {
        private readonly IProyectoService _proyectoService;
        private readonly IProyectoDashboardService _dashboardService;
        private readonly ApplicationDbContext _dbContext;

        public ProyectoController(
            IProyectoService proyectoService,
            IProyectoDashboardService dashboardService,
            ApplicationDbContext dbContext)
        {
            _proyectoService = proyectoService;
            _dashboardService = dashboardService;
            _dbContext = dbContext;
        }

        // --- ACCIÓN PRINCIPAL (GET: /Proyecto/Index) ---
        // Esta es la página ANFITRIONA del módulo
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 1. Cargar los KPIs del Header
            ViewBag.DashboardData = await _dashboardService.GetDashboardSummaryAsync();

            // 2. Cargar la lista inicial de Proyectos
            var proyectos = await _proyectoService.GetAllAsync();

            // Pasamos la lista como el Modelo principal de la vista
            return View(proyectos);
        }

        // --- ACCIÓN PARA RECARGAR LA TABLA DE PROYECTOS ---
        // (Usada por el AJAX después de guardar un modal)
        [HttpGet]
        public async Task<IActionResult> ListaProyectosPartial()
        {
            var proyectos = await _proyectoService.GetAllAsync();
            return PartialView("_ListaProyectosPartial", proyectos);
        }

        // --- ACCIONES PARA EL MODAL (Crear/Editar) ---

        [HttpGet]
        public async Task<IActionResult> GetProyectoParaEditar(int id)
        {
            var proyecto = (id == 0)
                ? new ProyectoDto { Estado = "Planificación" } // Nuevo proyecto
                : await _proyectoService.GetByIdAsync(id);  // Proyecto existente

            if (proyecto == null) return NotFound();

            // Devolvemos el formulario en una vista parcial
            return PartialView("_ProyectoFormPartial", proyecto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProyectoDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Datos inválidos." });

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _proyectoService.CreateAsync(dto);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = "Proyecto creado." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProyectoDto dto)
        {
            if (id != dto.Id)
                return Json(new { success = false, message = "Error de ID." });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Datos inválidos." });

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var updated = await _proyectoService.UpdateAsync(id, dto);
                if (updated == null)
                    return Json(new { success = false, message = "Proyecto no encontrado." });

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, message = "Proyecto actualizado." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}