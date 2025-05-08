using EmployeeBOApp.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeBOApp.Controllers
{
  
    public class LoginController : Controller
    {
        private readonly EmployeeDatabaseContext _context;

        public LoginController(EmployeeDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string emailId)
        {
            var user = _context.Logins.FirstOrDefault(u => u.EmailId == emailId);

            if (user != null && (user.Role == "PM" || user.Role == "DM" || user.Role == "GDO"))
            {
                // Store session info
                HttpContext.Session.SetString("EmailId", user.EmailId);
                HttpContext.Session.SetString("UserName", user.Username ?? "");

                // Create identity and sign in
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.EmailId),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserName", user.Username ?? "")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Only PM/DM/GDO members can access the application.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user and clear session
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();

            // Redirect to login page after logout
            return RedirectToAction("Login");
        }
    }
}
