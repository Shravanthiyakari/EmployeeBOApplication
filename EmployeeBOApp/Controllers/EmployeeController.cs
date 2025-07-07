using EmployeeBOApp.BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeBOApp.Controllers
{
    [Authorize(Roles = "PM,HR")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public IActionResult Index(string selectedProjectId = "Select")
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
                return Unauthorized();

            var projects = _employeeService.GetUserProjects(email, role);
            var employees = _employeeService.GetEmployees(selectedProjectId);

            ViewBag.ProjectListAll = projects;
            ViewBag.SelectedProjectId = selectedProjectId;

            return View(employees);
        }
    }
}
