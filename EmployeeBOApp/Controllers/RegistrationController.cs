using EmployeeBOApp.BusinessLayer.Interfaces;
using EmployeeBOApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeBOApp.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly IRegistrationService _registrationService;

        public RegistrationController(IRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Login model)
        {
            if (ModelState.IsValid)
            {
                var result = await _registrationService.RegisterUserAsync(model);
                if (result)
                {
                    TempData["SuccessMessage"] = "Successfully registered!";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "Registration failed.");
            }

            return View(model);
        }
    }
}
