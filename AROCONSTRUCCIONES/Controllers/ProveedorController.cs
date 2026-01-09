using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; // <-- Lo mantenemos para el 'catch'
using Microsoft.EntityFrameworkCore; // <-- Lo mantenemos para el 'catch'
using System;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    // Permitir que todos los roles logueados vean el Dashboard
    [Authorize(Roles = "Administrador,Usuario")]
    public class ProveedorController : Controller
    {
        private readonly IProveedorService _proveedorService;
        // Constructor actualizado
        public ProveedorController(IProveedorService proveedorService) 
        {
            _proveedorService = proveedorService;
        }

        [HttpGet]
        public async Task<IActionResult> ListaProveedores()
        {
            var proveedores = await _proveedorService.GetAllAsync();
            return PartialView("_ListaProveedoresPartial", proveedores);
        }

        [HttpGet]
        public async Task<IActionResult> GetProveedorParaEditar(int id)
        {
            var viewModel = await _proveedorService.GetEdicionProveedorAsync(id);
            if (viewModel == null) return NotFound();
            return PartialView("_ProveedorFormPartial", viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Pasamos ID 0 al servicio para que nos devuelva un ViewModel limpio para un nuevo proveedor
            var viewModel = await _proveedorService.GetEdicionProveedorAsync(0);
            return PartialView("_ProveedorFormPartial", viewModel);
        }

        // --- ACCIONES DE ESCRITURA (AHORA "THIN") ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProveedorEdicionViewModelDto vm)
        {
            if (!ModelState.IsValid)
            {
                // Si falla, recarga el ViewModel y devuelve el Partial
                var viewModelRecargado = await _proveedorService.GetEdicionProveedorAsync(0);
                vm.TodosLosMateriales = viewModelRecargado.TodosLosMateriales;
                vm.CategoriasMateriales = viewModelRecargado.CategoriasMateriales;
                return PartialView("_ProveedorFormPartial", vm);
            }

            // El bloque 'using var transaction' DESAPARECE
            try
            {
                // El servicio ahora toma el VM completo y guarda todo
                await _proveedorService.CreateAsync(vm); // <-- CAMBIO
                return Json(new { success = true, message = "Proveedor creado." });
            }
            catch (DbUpdateException ex) // Captura errores de BD (ej. RUC duplicado)
            {
                // Esta lógica de revisar el error SQL la podemos dejar aquí o en el servicio
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
                {
                    return Json(new { success = false, message = "Error: Ya existe un proveedor con ese RUC." });
                }
                return Json(new { success = false, message = "Error de base de datos: " + ex.InnerException?.Message ?? ex.Message });
            }
            catch (ApplicationException appEx) // Captura errores de negocio
            {
                return Json(new { success = false, message = appEx.Message });
            }
            catch (Exception ex) // Captura cualquier otro error
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProveedorEdicionViewModelDto vm)
        {
            if (!ModelState.IsValid)
            {
                var viewModelRecargado = await _proveedorService.GetEdicionProveedorAsync(vm.Proveedor.Id);
                vm.TodosLosMateriales = viewModelRecargado.TodosLosMateriales;
                vm.CategoriasMateriales = viewModelRecargado.CategoriasMateriales;
                return PartialView("_ProveedorFormPartial", vm);
            }

            try
            {
                // El servicio se encarga de la transacción y el guardado
                await _proveedorService.UpdateProveedorCompletoAsync(vm); // <-- CAMBIO
                return Json(new { success = true, message = "Proveedor actualizado." });
            }
            catch (DbUpdateException ex) // Captura errores de BD (ej. RUC duplicado)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
                {
                    return Json(new { success = false, message = "Error: El RUC ingresado ya pertenece a otro proveedor." });
                }
                return Json(new { success = false, message = "Error de base de datos: " + ex.InnerException?.Message ?? ex.Message });
            }
            catch (ApplicationException appEx) // Captura errores de negocio
            {
                return Json(new { success = false, message = appEx.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            // El bloque 'using var transaction' DESAPARECE
            try
            {
                var result = await _proveedorService.DeactivateAsync(id); // <-- CAMBIO
                if (!result)
                    return Json(new { success = false, message = "Proveedor no encontrado." });

                TempData["OpenTab"] = "#proveedores-tab";
                return Json(new { success = true, message = "Proveedor desactivado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}