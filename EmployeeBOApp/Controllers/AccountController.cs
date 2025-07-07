using EmployeeBOApp.BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeBOApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Submit()
        {
            _accountService.SubmitAccount();

            TempData["Message"] = "Details submitted successfully!";
            return RedirectToAction("Index");
        }
    }
}
