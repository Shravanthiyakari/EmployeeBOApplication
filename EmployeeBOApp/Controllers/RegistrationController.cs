using Microsoft.AspNetCore.Mvc;

namespace EmployeeBOApp.Controllers
{
    public class RegistrationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
