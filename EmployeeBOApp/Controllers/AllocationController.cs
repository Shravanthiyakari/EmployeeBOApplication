using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using EmployeeBOApp.Data;

namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class AllocationController : Controller
    {
        private readonly EmployeeDatabaseContext _context;
        private readonly IAllocationRepository _employeeRepository;

        public AllocationController(EmployeeDatabaseContext context,IAllocationRepository employeeRepository)
        {
            _context = context;
            _employeeRepository = employeeRepository;
        }

        public IActionResult Index()
        {
            var currentUserEmail = User.Identity?.Name;
            var shortProjectNames = _employeeRepository.GetShortProjectNames(currentUserEmail!);
            ViewBag.ShortProjectNames = shortProjectNames;
            return View();
        }

        [HttpGet]
        public JsonResult GetProjectDetails(string shortProjectName)
        {
            var result = _employeeRepository.GetProjectDetailsByShortName(shortProjectName);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRequest([FromForm] TicketingTable ticket)
        {
            var currentUserEmail = User.Identity?.Name;
            var (success, message) = await _employeeRepository.SubmitAllocationRequest(ticket, currentUserEmail!);
            return Json(new { success, message });
        }
    }
}
