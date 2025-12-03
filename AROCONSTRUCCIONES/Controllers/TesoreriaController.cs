using AROCONSTRUCCIONES.Dtos;
using AROCONSTRUCCIONES.Models;
using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    // Solo el Administrador (y quizás un futuro rol "Contador") puede entrar aquí
    [Authorize(Roles = "Administrador")]
    public class TesoreriaController : Controller
    {
        private readonly ITesoreriaService _tesoreriaService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFinanzasService _finanzasService;

        public TesoreriaController(ITesoreriaService tesoreriaService, UserManager<ApplicationUser> userManager, IFinanzasService finanzasService)
        {
            _tesoreriaService = tesoreriaService;
            _userManager = userManager;
            _finanzasService = finanzasService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(); // Carga la vista contenedora (Dashboard)
        }

        [HttpGet]
        public async Task<IActionResult> ListaSolicitudesPartial()
        {
            var solicitudes = await _tesoreriaService.GetAllSolicitudesAsync();
            return PartialView("_ListaSolicitudesPartial", solicitudes);
        }
        [HttpGet]
        public async Task<IActionResult> GetModalPago(int id, string codigo, decimal monto)
        {
            // 4. CARGAR CUENTAS BANCARIAS REALES
            var cuentas = await _finanzasService.GetAllCuentasAsync();
            var cuentasActivas = cuentas.Where(c => c.Activo);

            // Creamos el SelectList para la vista
            // Text: "BCP Soles - S/ 1,500.00" (Mostramos saldo para ayudar a decidir)
            ViewBag.CuentasBancarias = cuentasActivas.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(), // <-- AHORA USAMOS EL ID
                Text = $"{c.BancoNombre} ({c.Moneda}) - Saldo: {c.SaldoActual:N2}"
            }).ToList();

            var dto = new PagarSolicitudDto
            {
                SolicitudId = id,
                FechaPago = DateTime.Now
            };

            ViewBag.CodigoSP = codigo;
            ViewBag.Monto = monto;

            return PartialView("_ModalPagarSolicitud", dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPago(PagarSolicitudDto dto)
        {
            if (!ModelState.IsValid)
            {
                // 1. RECARGAR LA LISTA DE BANCOS (Si no, el select sale vacío y da error)
                var cuentas = await _finanzasService.GetAllCuentasAsync();
                var cuentasActivas = cuentas.Where(c => c.Activo);
                ViewBag.CuentasBancarias = cuentasActivas.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.BancoNombre} ({c.Moneda}) - Saldo: {c.SaldoActual:N2}"
                }).ToList();

                // Pasamos datos visuales extra
                // Nota: Como el DTO no tiene estos campos, si quieres mostrarlos tras un error
                // deberías pasarlos de nuevo o recuperarlos. Por simplicidad, ponemos un texto genérico.
                ViewBag.CodigoSP = "SP-REF";
                ViewBag.Monto = 0;

                // 2. DEVOLVER VISTA PARCIAL (HTML)
                return PartialView("_ModalPagarSolicitud", dto);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                await _tesoreriaService.RegistrarPagoAsync(dto, user.Id);

                TempData["SuccessMessage"] = "Pago registrado correctamente.";
                TempData["OpenTab"] = "#tesoreria-content";

                return Json(new { success = true, message = "Pago registrado." });
            }
            catch (Exception ex)
            {
                // Esto ahora será capturado por el "CASO B" del JavaScript y mostrará una alerta
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            try
            {
                var urlPdf = await _tesoreriaService.ObtenerUrlPdfSolicitud(id);
                // Redirigimos al archivo estático generado
                return Redirect(urlPdf);
            }
            catch (Exception ex)
            {
                return Content($"Error generando PDF: {ex.Message}");
            }
        }
    }
}