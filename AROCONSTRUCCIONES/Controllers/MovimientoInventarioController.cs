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
    public class MovimientoInventarioController : Controller
    {
        private readonly IMovimientoInventarioServices _movimientoInventarioServices;
        private readonly IAlmacenService _almacenService;
        private readonly IPresupuestoService _presupuestoService;
        private readonly IMaterialServices _materialService; // Requerido para modales
        private readonly IProyectoService _proyectoService;   // Requerido para modales
        private readonly IProveedorService _proveedorService; // Requerido para modales

        public MovimientoInventarioController(
                IMovimientoInventarioServices movimientoInventarioServices,
                IAlmacenService almacenService,
                IPresupuestoService presupuestoService,
                IMaterialServices materialService,
                IProyectoService proyectoService,
                IProveedorService proveedorService)
        {
            _movimientoInventarioServices = movimientoInventarioServices;
            _almacenService = almacenService;
            _presupuestoService = presupuestoService;
            _materialService = materialService;
            _proyectoService = proyectoService;
            _proveedorService = proveedorService;
        }

        // --- VISTAS PARCIALES Y LISTADOS ---

        [HttpGet]
        public async Task<IActionResult> TablaMovimientosPartial()
        {
            try
            {
                var almacenes = await _almacenService.GetAllActiveAsync();
                ViewBag.Almacenes = new SelectList(almacenes, "Id", "Nombre");

                var movimientos = await _movimientoInventarioServices.GetAllMovimientosAsync();

                return PartialView("TablaMovimientosPartial", movimientos);
            }
            catch (Exception ex)
            {
                ViewBag.Almacenes = new SelectList(new List<AlmacenDto>(), "Id", "Nombre");
                TempData["ErrorMessage"] = $"⚠️ Error al cargar la lista de movimientos: {ex.Message}";
                return PartialView("TablaMovimientosPartial", new List<MovimientoInventarioDto>());
            }
        }

        // --- CARGA DINÁMICA DE MODALES (AJAX) ---

        [HttpGet]
        public async Task<IActionResult> GetMovimientoModal(string tipo)
        {
            var dto = new MovimientoInventarioDto
            {
                TipoMovimiento = tipo.ToUpper(),
                FechaMovimiento = DateTime.Now
            };

            // Llenamos los ViewBags que requiere tu _MovimientoUnificadoModalPartial
            ViewBag.Almacenes = new SelectList(await _almacenService.GetAllActiveAsync(), "Id", "Nombre");

            var materiales = await _materialService.GetAllActiveAsync();
            ViewBag.Materiales = materiales.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = $"{m.Codigo} - {m.Nombre}"
            });

            ViewBag.Proveedores = new SelectList(await _proveedorService.GetAllAsync(), "Id", "RazonSocial");
            ViewBag.Proyectos = new SelectList(await _proyectoService.GetAllAsync(), "Id", "NombreProyecto");

            return PartialView("_MovimientoUnificadoModalPartial", dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetTransferenciaModal()
        {
            // Datos para el modal de transferencia entre almacenes
            ViewBag.Almacenes = new SelectList(await _almacenService.GetAllActiveAsync(), "Id", "Nombre");
            ViewBag.Materiales = new SelectList(await _materialService.GetAllActiveAsync(), "Id", "Nombre");

            return PartialView("_TransferenciaModalPartial", new TransferenciaDto());
        }

        [HttpGet]
        public async Task<IActionResult> GetPartidasPorProyecto(int proyectoId)
        {
            var partidas = await _presupuestoService.GetPartidasPorProyectoAsync(proyectoId);

            var items = partidas
                .Where(p => !p.EsTitulo)
                .Select(p => new
                {
                    value = p.Id,
                    text = $"{p.Item} - {p.Descripcion}"
                })
                .ToList();

            return Json(items);
        }

        // --- PROCESAMIENTO DE MOVIMIENTOS (POST) ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarIngreso(MovimientoInventarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
                return Json(new { success = false, message = $"Datos inválidos: {error}" });
            }

            try
            {
                dto.ResponsableNombre = User.Identity?.Name ?? "Usuario Sistema";
                bool resultado = await _movimientoInventarioServices.RegistrarIngreso(dto);

                return Json(new { success = resultado, message = resultado ? "¡Ingreso registrado exitosamente!" : "Error en el servicio." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSalida(MovimientoInventarioDto dto)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
                return Json(new { success = false, message = $"Datos inválidos: {error}" });
            }

            try
            {
                dto.ResponsableNombre = User.Identity?.Name ?? "Usuario Sistema";
                bool resultado = await _movimientoInventarioServices.RegistrarSalida(dto);

                return Json(new { success = resultado, message = resultado ? "¡Salida registrada exitosamente!" : "No se pudo completar la salida." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transferir(TransferenciaDto model)
        {
            if (model.AlmacenOrigenId == model.AlmacenDestinoId)
            {
                ModelState.AddModelError("AlmacenDestinoId", "El almacén de destino no puede ser el mismo que el de origen.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.ResponsableNombre = User.Identity?.Name ?? "Admin Sistema";
                    var resultado = await _movimientoInventarioServices.RealizarTransferenciaAsync(model);

                    if (resultado)
                    {
                        TempData["SuccessMessage"] = "Transferencia realizada con éxito.";
                        return RedirectToAction("Index", "Inventario");
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error en la transferencia: {ex.Message}";
                }
            }
            else
            {
                var msg = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                TempData["ErrorMessage"] = $"Datos inválidos: {msg}";
            }

            return RedirectToAction("Index", "Inventario");
        }
    }
}