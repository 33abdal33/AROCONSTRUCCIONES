using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AROCONSTRUCCIONES.Controllers
{
    // Permitir que todos los roles logueados vean el Dashboard
    [Authorize(Roles = "Administrador,Usuario,Almacenero")]
    public class DashboardController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }
    }
}
