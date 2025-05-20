using Microsoft.AspNetCore.Mvc;
using EmployeeBOApp.Models;
using EmployeeBOApp.Data;

namespace EmployeeBOApp.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly EmployeeDatabaseContext _context;

        public RegistrationController(EmployeeDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(Login model)
        {
            if (ModelState.IsValid)
            {
                _context.Logins.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Login", "Login"); // or a success page
            }

            return View(model);
        }
    }
}
