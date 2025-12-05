using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    [Authorize(Roles = "Administrador,Usuario")]
    public class PresupuestosController : Controller
    {
        private readonly IPresupuestoService _presupuestoService;
        private readonly IProyectoService _proyectoService;

        public PresupuestosController(IPresupuestoService presupuestoService, IProyectoService proyectoService)
        {
            _presupuestoService = presupuestoService;
            _proyectoService = proyectoService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int proyectoId = 0)
        {
            // Cargar proyectos para el filtro
            var proyectos = await _proyectoService.GetAllAsync();
            ViewBag.Proyectos = new SelectList(proyectos, "Id", "NombreProyecto");
            ViewBag.ProyectoIdSeleccionado = proyectoId;

            return View(); // Vista Principal
        }

        [HttpGet]
        public async Task<IActionResult> ListaPartidasPartial(int proyectoId)
        {
            if (proyectoId == 0) return Content("Seleccione un proyecto.");
            var partidas = await _presupuestoService.GetPartidasPorProyectoAsync(proyectoId);
            return PartialView("_ListaPartidasPartial", partidas);
        }

        [HttpPost]
        public async Task<IActionResult> ImportarExcel(int proyectoId, IFormFile archivoExcel)
        {
            if (proyectoId == 0) return Json(new { success = false, message = "Seleccione un proyecto." });
            if (archivoExcel == null || archivoExcel.Length == 0) return Json(new { success = false, message = "Seleccione un archivo válido." });

            try
            {
                using (var stream = archivoExcel.OpenReadStream())
                {
                    await _presupuestoService.ImportarPresupuestoDesdeExcelAsync(stream, proyectoId);
                }
                return Json(new { success = true, message = "Presupuesto importado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al importar: " + ex.Message });
            }
        }
    }
}