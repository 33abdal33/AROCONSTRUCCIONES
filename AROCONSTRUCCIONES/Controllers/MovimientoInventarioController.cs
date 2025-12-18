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

        public MovimientoInventarioController(
                IMovimientoInventarioServices movimientoInventarioServices,
                IAlmacenService almacenService,
                IPresupuestoService presupuestoService)
        {
            _movimientoInventarioServices = movimientoInventarioServices;
            _almacenService = almacenService;
            _presupuestoService = presupuestoService;
        }

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
                // Asignar responsable si no viene del front (Opcional, pero recomendado)
                dto.ResponsableNombre = User.Identity?.Name ?? "Usuario Sistema";

                bool resultado = await _movimientoInventarioServices.RegistrarIngreso(dto);

                if (resultado)
                    return Json(new { success = true, message = "¡Ingreso registrado exitosamente!" });
                else
                    return Json(new { success = false, message = "Error en el servicio." });
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
                // Asignar responsable
                dto.ResponsableNombre = User.Identity?.Name ?? "Usuario Sistema";

                bool resultado = await _movimientoInventarioServices.RegistrarSalida(dto);

                if (resultado)
                    return Json(new { success = true, message = "¡Salida registrada exitosamente!" });
                else
                    return Json(new { success = false, message = "No se pudo completar la salida." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // ⭐ NUEVA ACCIÓN: TRANSFERENCIA (Integrated)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transferir(TransferenciaDto model)
        {
            // 1. Validación de lógica de negocio en el Controller
            if (model.AlmacenOrigenId == model.AlmacenDestinoId)
            {
                ModelState.AddModelError("AlmacenDestinoId", "El almacén de destino no puede ser el mismo que el de origen.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 2. Asignar Responsable (Usuario logueado)
                    model.ResponsableNombre = User.Identity?.Name ?? "Admin Sistema";

                    // 3. Llamar al Servicio
                    var resultado = await _movimientoInventarioServices.RealizarTransferenciaAsync(model);

                    if (resultado)
                    {
                        TempData["SuccessMessage"] = "Transferencia realizada con éxito.";
                        // Redirigimos al Index del Inventario para recargar la página y ver los cambios
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
                // Si el modelo es inválido, recogemos el primer error para mostrarlo
                var msg = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                TempData["ErrorMessage"] = $"Datos inválidos: {msg}";
            }

            // Si falla, volvemos al Inventario (los mensajes se mostrarán por TempData)
            return RedirectToAction("Index", "Inventario");
        }
    }
}