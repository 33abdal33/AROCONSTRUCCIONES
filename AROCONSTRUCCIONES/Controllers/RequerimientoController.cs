using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Services.Implementation;
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
    public class RequerimientoController : Controller
    {
        private readonly IRequerimientoService _requerimientoService;
        private readonly IProyectoService _proyectoService;
        private readonly IMaterialServices _materialService;
        private readonly ILogger<RequerimientoController> _logger; // <-- 2. AÑADIR

        public RequerimientoController(
          IRequerimientoService requerimientoService,
          IProyectoService proyectoService,
          IMaterialServices materialService, // <-- 1. AÑADIR
          ILogger<RequerimientoController> logger) // <-- 3. AÑADIR
        {
            _requerimientoService = requerimientoService;
            _proyectoService = proyectoService;
            _materialService = materialService;
            _logger = logger; // <-- 4. AÑADIR
        }

        // --- ACCIÓN PARA LLENAR LA PESTAÑA "REQUERIMIENTOS" (Global) ---
        [HttpGet]
        public async Task<IActionResult> ListaRequerimientosPartial()
        {
            _logger.LogInformation("--- [RequerimientoController] Iniciando ListaRequerimientosPartial ---");
            try
            {
                var requerimientos = await _requerimientoService.GetAllRequerimientosAsync();

                int count = requerimientos.Count();
                _logger.LogInformation($"[RequerimientoController] Servicio devolvió {count} requerimientos.");

                if (count == 0)
                {
                    _logger.LogWarning("[RequerimientoController] La lista de requerimientos está vacía. Se mostrará la tabla vacía.");
                }

                _logger.LogInformation("[RequerimientoController] Renderizando _ListaRequerimientosPartial...");
                return PartialView("_ListaRequerimientosPartial", requerimientos);
            }
            catch (Exception ex)
            {
                // ¡ESTO ES CLAVE!
                // Si algo falla (el servicio o el renderizado de la vista), 
                // devolvemos el error como HTML para que el AJAX lo muestre.
                _logger.LogError(ex, "[RequerimientoController] ERROR al obtener o renderizar ListaRequerimientosPartial.");
                return Content($"<div class='text-red-500 p-4'><b>Error al cargar la pestaña:</b><br>{ex.Message}<br><pre>{ex.StackTrace}</pre></div>");
            }
        }

        // --- ACCIÓN PARA MOSTRAR EL FORMULARIO MODAL DE CREACIÓN (Global) ---
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Llamamos al servicio para obtener el siguiente código disponible
            // Si aún no tienes esta función en el servicio, la definiremos abajo.
            string proximoCodigo = await _requerimientoService.GetNextCodigoAsync();

            var dto = new RequerimientoCreateDto
            {
                Fecha = DateTime.Now,
                Codigo = proximoCodigo // Ahora es correlativo: REQ-0001, etc.
            };

            await CargarViewBagsFormulario();
            return PartialView("Create", dto);
        }

        // --- ACCIÓN PARA GUARDAR EL NUEVO REQUERIMIENTO (DESDE EL MODAL) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RequerimientoCreateDto dto) // <-- CAMBIO A DTO COMPLETO
        {
            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                ModelState.AddModelError("Detalles", "Debe añadir al menos un material al requerimiento.");
            }

            if (!ModelState.IsValid)
            {
                await CargarViewBagsFormulario();
                return PartialView("Create", dto);
            }

            try
            {
                await _requerimientoService.CreateAsync(dto);
                TempData["OpenTab"] = "#requerimientos-tab";
                return Json(new { success = true, message = "Requerimiento creado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- HELPER (ACTUALIZADO) ---
        private async Task CargarViewBagsFormulario()
        {
            // Carga Proyectos
            var proyectos = await _proyectoService.GetAllAsync();
            ViewBag.Proyectos = new SelectList(proyectos.Where(p => p.Estado != "Finalizado"), "Id", "NombreProyecto");

            // Carga Prioridades
            ViewBag.Prioridades = new SelectList(new List<string> { "Baja", "Media", "Alta", "Urgente" });

            // --- AÑADIDO ---
            // Carga Materiales para el dropdown de detalles
            var materiales = await _materialService.GetAllActiveAsync();
            ViewBag.Materiales = materiales.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.Codigo} - {m.Nombre} ({m.UnidadMedida})"
            }).ToList();
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation($"[RequerimientoController] Solicitando modal de Detalles para ID: {id}");

            var dto = await _requerimientoService.GetRequerimientoDetailsAsync(id);

            if (dto == null)
            {
                _logger.LogWarning($"[RequerimientoController] Detalles no encontrados para ID: {id}");
                return NotFound();
            }

            // Devolvemos la nueva vista parcial que crearemos en el siguiente paso
            return PartialView("_DetailsModalPartial", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // ¡Seguridad! Solo Admin y Usuario pueden aprobar
        [Authorize(Roles = "Administrador,Usuario")]
        public async Task<IActionResult> Approve(int id)
        {
            _logger.LogInformation($"[RequerimientoController] Recibida solicitud POST para Aprobar ID: {id}");
            try
            {
                var result = await _requerimientoService.ApproveAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "No se pudo aprobar. El requerimiento no fue encontrado o ya fue procesado." });
                }

                // Si es exitoso, le decimos a la pestaña que se recargue
                TempData["OpenTab"] = "#requerimientos-tab";
                return Json(new { success = true, message = "Requerimiento Aprobado. Listo para Compras." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RequerimientoController] Error al aprobar.");
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Usuario")]
        public async Task<IActionResult> Cancel(int id)
        {
            _logger.LogInformation($"[RequerimientoController] Solicitud de cancelación para ID: {id}");
            try
            {
                var result = await _requerimientoService.CancelAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "No se pudo cancelar. Puede que el requerimiento ya haya sido procesado o no exista." });
                }

                return Json(new { success = true, message = "El requerimiento ha sido cancelado exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RequerimientoController] Error al cancelar.");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}