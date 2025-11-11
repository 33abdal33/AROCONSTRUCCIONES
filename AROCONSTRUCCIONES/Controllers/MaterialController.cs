using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Persistence; // <-- 1. Necesario para DbContext
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    public class MaterialController : Controller
    {
        private readonly IMaterialServices _materialService;
        private readonly ApplicationDbContext _dbContext; // <-- 2. Para la transacción

        public MaterialController(
            IMaterialServices materialService,
            ApplicationDbContext dbContext) // <-- 3. Inyectar DbContext
        {
            _materialService = materialService;
            _dbContext = dbContext; // <-- 4. Asignar DbContext
        }

        // ACCIÓN PARA LA PESTAÑA (Tu código original, está perfecto)
        [HttpGet]
        public async Task<IActionResult> ListaMateriales()
        {
            try
            {
                var materiales = await _materialService.GetAllAsync(); // Traer todos (activos e inactivos)

                // --- Cargar ViewBag para los modales de esta pestaña ---
                await LoadModalViewBags();

                return PartialView("ListaMaterialesPartial", materiales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error en la carga del catálogo: {ex.Message}");
            }
        }

        // --- REFACTORIZADO PARA MODAL AJAX ---
        // POST: /Material/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaterialDto dto)
        {
            if (!ModelState.IsValid)
            {
                var error = "Error de validación. Revisa los campos.";
                return Json(new { success = false, message = error });
            }

            // Usamos el mismo patrón de transacción que en tus otros servicios
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _materialService.CreateAsync(dto);
                await _dbContext.SaveChangesAsync(); // <-- Guardamos los cambios aquí
                await transaction.CommitAsync();

                // Pasamos el TempData para que el JS sepa qué pestaña abrir
                TempData["OpenTab"] = "#materiales-tab";

                return Json(new { success = true, message = "¡Material creado exitosamente!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- REFACTORIZADO PARA MODAL AJAX ---
        // GET: /Material/GetMaterialParaEditar/5
        [HttpGet]
        public async Task<IActionResult> GetMaterialParaEditar(int id)
        {
            var material = (id == 0)
                ? new MaterialDto { Estado = true } // Nuevo material por defecto
                : await _materialService.GetByIdAsync(id); // O material existente

            if (material == null)
                return NotFound();

            // Cargamos los ViewBags para el modal de edición
            await LoadModalViewBags();

            // Devolvemos la vista parcial del formulario de edición
            return PartialView("_MaterialFormPartial", material);
        }

        // --- REFACTORIZADO PARA MODAL AJAX ---
        // POST: /Material/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MaterialDto dto)
        {
            if (id != dto.Id)
                return Json(new { success = false, message = "Error de ID." });

            if (!ModelState.IsValid)
            {
                var error = "Error de validación. Revisa los campos.";
                return Json(new { success = false, message = error });
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var updated = await _materialService.UpdateAsync(id, dto);
                if (updated == null)
                    return Json(new { success = false, message = "Material no encontrado." });

                await _dbContext.SaveChangesAsync(); // <-- Guardamos los cambios aquí
                await transaction.CommitAsync();

                TempData["OpenTab"] = "#materiales-tab";

                return Json(new { success = true, message = "¡Material actualizado!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- REFACTORIZADO PARA MODAL AJAX ---
        // POST: /Material/Delete/5 (Ahora es Deactivate)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                bool resultado = await _materialService.DeactivateAsync(id);
                if (!resultado)
                    return Json(new { success = false, message = "Material no encontrado." });

                await _dbContext.SaveChangesAsync(); // <-- Guardamos los cambios aquí
                await transaction.CommitAsync();

                TempData["OpenTab"] = "#materiales-tab";

                return Json(new { success = true, message = "Material desactivado." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- HELPER ---
        private async Task LoadModalViewBags()
        {
            var categorias = (await _materialService.GetMaterialCategoriesAsync()) ?? new List<string>();
            var unidades = (await _materialService.GetMaterialUnitsAsync()) ?? new List<string>();

            ViewBag.Categorias = new SelectList(categorias);
            ViewBag.Unidades = new SelectList(unidades);
        }
    }
}