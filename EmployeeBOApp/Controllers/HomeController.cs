using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeBOApp.Controllers
{
    [Authorize]  // Ensure only authenticated users can access this controller
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
