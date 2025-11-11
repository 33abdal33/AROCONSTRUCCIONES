using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Persistence; // <-- 1. Necesario para DbContext
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly IProveedorService _proveedorService;
        private readonly ApplicationDbContext _dbContext; // <-- 2. Para la transacción

        public ProveedorController(IProveedorService proveedorService, ApplicationDbContext dbContext)
        {
            _proveedorService = proveedorService;
            _dbContext = dbContext; // <-- 3. Asignar DbContext
        }

        // ACCIÓN PARA LA PESTAÑA (OK)
        [HttpGet]
        public async Task<IActionResult> ListaProveedores()
        {
            var proveedores = await _proveedorService.GetAllAsync();
            return PartialView("_ListaProveedoresPartial", proveedores);
        }

        [HttpGet]
        public async Task<IActionResult> GetProveedorParaEditar(int id)
        {
            // Llama al nuevo método del servicio que prepara el ViewModel
            var viewModel = await _proveedorService.GetEdicionProveedorAsync(id);
            if (viewModel == null) return NotFound();

            // Devuelve el formulario (con pestañas)
            return PartialView("_ProveedorFormPartial", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProveedorEdicionViewModelDto vm)
        {
            if (!ModelState.IsValid)
            {
                // ¡CORREGIDO! Si falla, recarga el ViewModel y devuelve el Partial
                var viewModelRecargado = await _proveedorService.GetEdicionProveedorAsync(0); // Recarga listas
                vm.TodosLosMateriales = viewModelRecargado.TodosLosMateriales; // Transfiere las listas
                vm.CategoriasMateriales = viewModelRecargado.CategoriasMateriales;
                return PartialView("_ProveedorFormPartial", vm); // Devuelve el form con errores
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _proveedorService.CreateAsync(vm.Proveedor);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Proveedor creado." });
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
                {
                    return Json(new { success = false, message = "Error: Ya existe un proveedor con ese RUC." });
                }
                return Json(new { success = false, message = "Error de base de datos: " + ex.InnerException?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Proveedor/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProveedorEdicionViewModelDto vm)
        {
            if (!ModelState.IsValid)
            {
                // ¡CORREGIDO! Si falla, recarga el ViewModel y devuelve el Partial
                var viewModelRecargado = await _proveedorService.GetEdicionProveedorAsync(vm.Proveedor.Id); // Recarga listas
                vm.TodosLosMateriales = viewModelRecargado.TodosLosMateriales; // Transfiere las listas
                vm.CategoriasMateriales = viewModelRecargado.CategoriasMateriales;
                return PartialView("_ProveedorFormPartial", vm); // Devuelve el form con errores
            }

            try
            {
                await _proveedorService.UpdateProveedorCompletoAsync(vm);
                return Json(new { success = true, message = "Proveedor actualizado." });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
                {
                    return Json(new { success = false, message = "Error: El RUC ingresado ya pertenece a otro proveedor." });
                }
                return Json(new { success = false, message = "Error de base de datos: " + ex.InnerException?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /Proveedor/Deactivate/5 (Esta acción está bien)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var result = await _proveedorService.DeactivateAsync(id);
                if (!result)
                    return Json(new { success = false, message = "Proveedor no encontrado." });

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["OpenTab"] = "#proveedores-tab";
                return Json(new { success = true, message = "Proveedor desactivado." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}