using AROCONSTRUCCIONES.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AROCONSTRUCCIONES.Controllers
{
    // Mantenemos los roles para que todos los interesados vean las métricas
    [Authorize(Roles = "Administrador,Usuario,Almacenero")]
    public class DashboardController : Controller
    {
        private readonly ILogisticaDashboardService _dashboardService;

        public DashboardController(ILogisticaDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // --- ACCIÓN AJAX: Entrega los datos para Chart.js ---
        [HttpGet]
        public async Task<IActionResult> GetLogisticaChartData()
        {
            // El controlador es ligero: solo pide y entrega
            var data = await _dashboardService.GetLogisticaChartDataAsync();
            return Json(data);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}