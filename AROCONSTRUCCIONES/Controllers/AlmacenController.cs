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
        private readonly ApplicationDbContext _dbContext;

        public AlmacenController(IAlmacenService almacenService, ApplicationDbContext dbContext)
        {
            _almacenService = almacenService;
            _dbContext = dbContext;
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

        // ACCIÓN PARA CREAR (AJAX POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlmacenDto dto)
        {
            if (!ModelState.IsValid)
            {
                // ¡CORREGIDO! Devuelve el PartialView con los errores
                return PartialView("_AlmacenFormPartial", dto);
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _almacenService.CreateAsync(dto);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Almacén creado." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ACCIÓN PARA EDITAR (AJAX POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AlmacenDto dto)
        {
            if (id != dto.Id)
                return Json(new { success = false, message = "Error de ID." });

            if (!ModelState.IsValid)
            {
                // ¡CORREGIDO! Devuelve el PartialView con los errores
                return PartialView("_AlmacenFormPartial", dto);
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var updated = await _almacenService.UpdateAsync(id, dto);
                if (updated == null)
                    return Json(new { success = false, message = "Almacén no encontrado." });

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Almacén actualizado." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ACCIÓN PARA DESACTIVAR (AJAX POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var result = await _almacenService.DeactivateAsync(id);
                if (!result)
                    return Json(new { success = false, message = "Almacén no encontrado." });

                await _dbContext.SaveChangesAsync(); // El Controlador guarda
                await transaction.CommitAsync();

                TempData["OpenTab"] = "#almacenes-tab";
                return Json(new { success = true, message = "Almacén desactivado." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                if (ex.InnerException != null && ex.InnerException.Message.Contains("REFERENCE constraint"))
                {
                    return Json(new { success = false, message = "No se puede desactivar. El almacén tiene inventario o movimientos asociados." });
                }
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}