using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Services.Interface;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic; // <-- Asegúrate de tener este 'using'
using System.Linq; // <-- Asegúrate de tener este 'using'
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AROCONSTRUCCIONES.Controllers
{
    public class RequerimientoController : Controller
    {
        private readonly IRequerimientoService _requerimientoService;
        private readonly IProyectoService _proyectoService;
        private readonly IMaterialServices _materialService;

        public RequerimientoController(
          IRequerimientoService requerimientoService,
          IProyectoService proyectoService,
          IMaterialServices materialService)
        {
            _requerimientoService = requerimientoService;
            _proyectoService = proyectoService;
            _materialService = materialService;
        }

        // --- ACCIÓN PARA LLENAR LA PESTAÑA "REQUERIMIENTOS" ---
        [HttpGet]
        public async Task<IActionResult> ListaRequerimientosPartial(int proyectoId)
        {
            // Si no hay proyectoId (ej. no hay proyectos creados), devuelve una lista vacía
            if (proyectoId == 0)
            {
                // Pasa el ID del proyecto a la vista parcial para que el botón "Nuevo" lo sepa
                ViewBag.ProyectoId = 0;
                return PartialView("_ListaRequerimientosPartial", new List<RequerimientoListDto>());
            }

            var requerimientos = await _requerimientoService.GetRequerimientosPorProyectoAsync(proyectoId);
            ViewBag.ProyectoId = proyectoId; // Pasa el ID del proyecto a la vista
            return PartialView("_ListaRequerimientosPartial", requerimientos);
        }

        // --- ACCIÓN PARA MOSTRAR EL FORMULARIO DE CREACIÓN ---
        // GET: /Requerimiento/Create?proyectoId=5
        [HttpGet]
        public async Task<IActionResult> Create(int proyectoId)
        {
            // --- ¡INICIO DE LA CORRECCIÓN 1! ---
            if (proyectoId == 0)
            {
                TempData["Error"] = "Error: No se ha especificado un proyecto. Intente de nuevo desde la pestaña de Proyectos.";
                // Lo redirigimos a la página principal de Proyectos
                return RedirectToAction("Index", "Proyecto");
            }
            // --- FIN DE LA CORRECCIÓN 1 ---

            var dto = new RequerimientoCreateDto
            {
                IdProyecto = proyectoId
            };

            await CargarViewBagsFormulario(proyectoId);
            return View(dto);
        }

        // --- ACCIÓN PARA GUARDAR EL NUEVO REQUERIMIENTO ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RequerimientoCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos. Revisa el formulario.";
                await CargarViewBagsFormulario(dto.IdProyecto);
                return View(dto);
            }

            try
            {
                await _requerimientoService.CreateAsync(dto);

                TempData["Exito"] = "¡Requerimiento guardado exitosamente!";
                TempData["OpenTab"] = "#requerimientos-tab";

                // Redirigimos de vuelta al Index del Proyecto
                // ¡OJO! El ID del proyecto está en el DTO.
                return RedirectToAction("Index", "Proyecto");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al guardar: {ex.Message}";
                await CargarViewBagsFormulario(dto.IdProyecto);
                return View(dto);
            }
         }

        // --- HELPER (ACTUALIZADO) ---
         private async Task CargarViewBagsFormulario(int proyectoId)
        {
            var proyecto = await _proyectoService.GetByIdAsync(proyectoId);

            // --- ¡INICIO DE LA CORRECCIÓN 2! ---
            if (proyecto == null)
            {
                // Si el proyecto es nulo (ej. proyectoId = 0), creamos una lista vacía
                ViewBag.Proyectos = new SelectList(new List<ProyectoDto>(), "Id", "NombreProyecto");
            }
            else
            {
                // Si existe, lo añadimos a la lista
                ViewBag.Proyectos = new SelectList(new[] { proyecto }, "Id", "NombreProyecto", proyectoId);
            }
            // --- FIN DE LA CORRECCIÓN 2 ---

            var materiales = await _materialService.GetAllActiveAsync();
            ViewBag.Materiales = materiales.Select(m => new SelectListItem
      
           {
                Value = m.Id.ToString(),
                Text = $"{m.Codigo} - {m.Nombre} ({m.UnidadMedida})"
      
      }).ToList();
        }
    }
}