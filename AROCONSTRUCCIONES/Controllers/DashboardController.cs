using Microsoft.AspNetCore.Mvc;

namespace AROCONSTRUCCIONES.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
