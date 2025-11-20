using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // --- LOGIN (GET) ---
        // Muestra el formulario de inicio de sesión
        [HttpGet]
        [AllowAnonymous] // Permite que usuarios no logueados vean esta página
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // --- LOGIN (POST) ---
        // Procesa el formulario de inicio de sesión
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Intenta iniciar sesión con la contraseña.
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Si el login es exitoso, redirige a donde el usuario intentaba ir,
                    // o al Dashboard si no intentaba ir a ningún lado.
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    // Si falla, muestra un error
                    ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido. Verifique su correo y contraseña.");
                    return View(model);
                }
            }
            // Si el modelo no es válido, vuelve a mostrar el formulario
            return View(model);
        }

        // --- LOGOUT (POST) ---
        // Cierra la sesión del usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            // Redirige al login después de cerrar sesión
            return RedirectToAction("Login", "Account");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Dashboard");
            }
        }
    }
}