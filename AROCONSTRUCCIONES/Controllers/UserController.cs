using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    // ¡¡SEGURIDAD!! Solo el Administrador puede entrar a este controlador
    [Authorize(Roles = "Administrador")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // --- MUESTRA EL FORMULARIO DE REGISTRO ---
        // (Usamos Index como la página principal de este módulo)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await CargarModeloDeRegistro();
            return View(model);
        }

        // --- PROCESA EL FORMULARIO DE REGISTRO ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    NombreCompleto = model.NombreCompleto,
                    EmailConfirmed = true // Se auto-confirma
                };

                // Intenta crear el usuario
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Si se crea, le asigna el rol seleccionado
                    await _userManager.AddToRoleAsync(user, model.RoleName);

                    TempData["SuccessMessage"] = $"¡Usuario '{user.Email}' creado exitosamente!";
                    return RedirectToAction("Index");
                }

                // Si falla (ej. email ya existe), añade los errores
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Si el modelo no es válido o la creación falló, recarga el formulario
            var viewModel = await CargarModeloDeRegistro(model);
            return View(viewModel);
        }

        // --- MÉTODO HELPER ---
        private async Task<RegisterDto> CargarModeloDeRegistro(RegisterDto model = null)
        {
            var roles = await _roleManager.Roles
                                .Select(r => new SelectListItem { Text = r.Name, Value = r.Name })
                                .ToListAsync();

            if (model == null)
            {
                return new RegisterDto { RolesList = roles };
            }

            // Si el modelo viene de un POST fallido, solo re-adjuntamos la lista de roles
            model.RolesList = roles;
            return model;
        }
    }
}