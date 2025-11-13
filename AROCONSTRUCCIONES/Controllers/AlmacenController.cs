using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Persistence;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; // Para el catch de errores SQL
using Microsoft.EntityFrameworkCore; // Para DbUpdateException
using System;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    public class AlmacenController : Controller
    {
        private readonly IAlmacenService _almacenService;
        // private readonly ApplicationDbContext _dbContext; // <-- SE VA

        // Constructor actualizado: Solo inyecta el servicio
        public AlmacenController(IAlmacenService almacenService) // <-- CAMBIO
        {
            _almacenService = almacenService;
            // _dbContext = dbContext; // <-- SE VA
        }
        // ACCIÓN PARA LA PESTAÑA (AJAX GET)
        [HttpGet]
        public async Task<IActionResult> ListaAlmacenes()
        {
            var almacenes = await _almacenService.GetAllAsync(); // Trae todos (activos e inactivos)
            return PartialView("_ListaAlmacenPartial", almacenes);
        }

        // ACCIÓN PARA OBTENER EL FORMULARIO (AJAX GET)
        // GET: /Almacen/GetAlmacenParaEditar/5 (o 0 para Crear)
        [HttpGet]
        public async Task<IActionResult> GetAlmacenParaEditar(int id)
        {
            var almacen = (id == 0)
              ? new AlmacenDto { Estado = true } // Nuevo almacén por defecto
                      : await _almacenService.GetByIdAsync(id); // O uno existente

            if (almacen == null) return NotFound();

            return PartialView("_AlmacenFormPartial", almacen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlmacenDto dto)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AlmacenFormPartial", dto);
            }

            // El bloque 'using var transaction' DESAPARECE
            try
            {
                // Solo llamas al servicio. El servicio se encarga de guardar.
                await _almacenService.CreateAsync(dto); // <-- CAMBIO
                return Json(new { success = true, message = "Almacén creado." });
            }
            catch (Exception ex)
            {
                // El Rollback es automático porque el servicio lanzó una excepción
                // antes de llamar a SaveChangesAsync()
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AlmacenDto dto)
        {
            if (id != dto.Id)
                return Json(new { success = false, message = "Error de ID." });

            if (!ModelState.IsValid)
            {
                return PartialView("_AlmacenFormPartial", dto);
            }

            // El bloque 'using var transaction' DESAPARECE
            try
            {
                var updated = await _almacenService.UpdateAsync(id, dto); // <-- CAMBIO
                if (updated == null)
                    return Json(new { success = false, message = "Almacén no encontrado." });

                return Json(new { success = true, message = "Almacén actualizado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ACCIÓN PARA DESACTIVAR (AJAX POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            // El bloque 'using var transaction' DESAPARECE
            try
            {
                var result = await _almacenService.DeactivateAsync(id); // <-- CAMBIO
                if (!result)
                    return Json(new { success = false, message = "Almacén no encontrado." });

                TempData["OpenTab"] = "#almacenes-tab";
                return Json(new { success = true, message = "Almacén desactivado." });
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("REFERENCE constraint"))
                {
                    return Json(new { success = false, message = "No se puede desactivar. El almacén tiene inventario o movimientos asociados." });
                }
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}