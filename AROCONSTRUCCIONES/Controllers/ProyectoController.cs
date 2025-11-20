using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    // Solo Admin y Usuario (Back-office) pueden ver Proyectos
    [Authorize(Roles = "Administrador,Usuario")]
    public class ProyectoController : Controller
    {
        private readonly IProyectoService _proyectoService;
        private readonly IProyectoDashboardService _dashboardService;

        public ProyectoController(
            IProyectoService proyectoService,
            IProyectoDashboardService dashboardService)
        {
            _proyectoService = proyectoService;
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.DashboardData = await _dashboardService.GetDashboardSummaryAsync();
            var proyectos = await _proyectoService.GetAllAsync();
            return View(proyectos);
        }

        [HttpGet]
        public async Task<IActionResult> ListaProyectosPartial()
        {
            var proyectos = await _proyectoService.GetAllAsync();
            return PartialView("_ListaProyectosPartial", proyectos);
        }

        [HttpGet]
        public async Task<IActionResult> GetProyectoParaEditar(int id)
        {
            var proyecto = (id == 0)
                ? new ProyectoDto { Estado = "Planificación" }
                : await _proyectoService.GetByIdAsync(id);

            if (proyecto == null) return NotFound();
            return PartialView("_ProyectoFormPartial", proyecto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProyectoDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Datos inválidos." });

            try
            {
                await _proyectoService.CreateAsync(dto);
                return Json(new { success = true, message = "Proyecto creado." });
            }
            catch (Exception ex)
            {
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

            try
            {
                var updated = await _proyectoService.UpdateAsync(id, dto);
                if (updated == null)
                    return Json(new { success = false, message = "Proyecto no encontrado." });

                return Json(new { success = true, message = "Proyecto actualizado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}