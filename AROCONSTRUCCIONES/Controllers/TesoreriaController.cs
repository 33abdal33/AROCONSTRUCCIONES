using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    // Solo el Administrador (y quizás un futuro rol "Contador") puede entrar aquí
    [Authorize(Roles = "Administrador")]
    public class TesoreriaController : Controller
    {
        private readonly ITesoreriaService _tesoreriaService;

        public TesoreriaController(ITesoreriaService tesoreriaService)
        {
            _tesoreriaService = tesoreriaService;
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
    }
}