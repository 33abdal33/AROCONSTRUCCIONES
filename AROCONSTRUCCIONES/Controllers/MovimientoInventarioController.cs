using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Necesario para SelectList
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    [Authorize(Roles = "Administrador,Usuario")]
    public class MovimientoInventarioController : Controller
    {
        private readonly IMovimientoInventarioServices _movimientoInventarioServices;
        // 🔹 Inyección del servicio de Almacén
        private readonly IAlmacenService _almacenService;
        private readonly IPresupuestoService _presupuestoService;

        public MovimientoInventarioController(
                IMovimientoInventarioServices movimientoInventarioServices,
                IAlmacenService almacenService,
                IPresupuestoService presupuestoService)
        {
            _movimientoInventarioServices = movimientoInventarioServices;
            _almacenService = almacenService; // 🔹 Inicializamos el servicio
            _presupuestoService = presupuestoService;
        }

        // ⭐ ACCIÓN RENOMBRADA Y VISTA ACTUALIZADA:
        // El nombre de la acción debe coincidir con el 'data-url' de la pestaña en Inventario/Index.
        [HttpGet]
        public async Task<IActionResult> TablaMovimientosPartial()
        {
            try
            {
                // 1. Cargar la lista de almacenes para el filtro de la vista parcial
                var almacenes = await _almacenService.GetAllActiveAsync(); // Usamos GetAllActiveAsync para filtros
                // Pasamos los almacenes como SelectList a ViewBag.Almacenes
                ViewBag.Almacenes = new SelectList(almacenes, "Id", "Nombre");

                // 2. Llamar al servicio para obtener todos los movimientos (Kárdex)
                var movimientos = await _movimientoInventarioServices.GetAllMovimientosAsync();

                // 3. Devolver la vista parcial
                return PartialView("TablaMovimientosPartial", movimientos);
            }
            catch (Exception ex)
            {
                // En caso de error, aseguramos que ViewBag.Almacenes sea una lista vacía para evitar fallos en la vista
                ViewBag.Almacenes = new SelectList(new List<AlmacenDto>(), "Id", "Nombre");
                TempData["ErrorMessage"] = $"⚠️ Error al cargar la lista de movimientos: {ex.Message}";
                // Devolver la vista parcial con un modelo vacío para que se cargue la estructura HTML
                return PartialView("TablaMovimientosPartial", new List<MovimientoInventarioDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPartidasPorProyecto(int proyectoId)
        {
            var partidas = await _presupuestoService.GetPartidasPorProyectoAsync(proyectoId);

            // Filtramos solo las que NO son títulos (porque no puedes gastar en un título)

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
            // 1. Validar el modelo
            if (!ModelState.IsValid)
            {
                // Si falla, devolvemos un JSON de error con los detalles
                var error = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
                return Json(new { success = false, message = $"Datos inválidos: {error}" });
            }

            try
            {
                // 2. Llamar al servicio
                bool resultado = await _movimientoInventarioServices.RegistrarIngreso(dto);

                if (resultado)
                {
                    // 3. Devolver JSON de éxito
                    return Json(new { success = true, message = "¡Ingreso registrado exitosamente!" });
                }
                else
                {
                    return Json(new { success = false, message = "Error en el servicio." });
                }
            }
            catch (Exception ex)
            {
                // 4. Devolver JSON con el error de negocio (ej. "Stock insuficiente")
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarSalida(MovimientoInventarioDto dto)
        {
            // 1. Validar el modelo
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
                return Json(new { success = false, message = $"Datos inválidos: {error}" });
            }

            // (Tu validación de cantidad > 0 es buena, pero el servicio ya debería hacerla)

            try
            {
                // 2. Llamar al servicio
                bool resultado = await _movimientoInventarioServices.RegistrarSalida(dto);

                if (resultado)
                {
                    // 3. Devolver JSON de éxito
                    return Json(new { success = true, message = "¡Salida registrada exitosamente!" });
                }
                else
                {
                    // Esto es redundante si el servicio lanza una excepción, pero está bien
                    return Json(new { success = false, message = "No se pudo completar la salida." });
                }
            }
            catch (Exception ex)
            {
                // 4. Devolver JSON con el error de negocio (ej. "Stock insuficiente")
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
