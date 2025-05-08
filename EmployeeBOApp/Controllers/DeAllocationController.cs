using EmployeeBOApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeBOApp.Repositories.Interfaces;
using EmployeeBOApp.Data;


namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class DeallocationController : Controller
    {
        private readonly IDeallocationRepository _deallocationRepo;
        private readonly EmployeeDatabaseContext _context;
        public DeallocationController(IDeallocationRepository deallocationRepo, EmployeeDatabaseContext context)
        {
            _deallocationRepo = deallocationRepo;
            _context = context;
            
        }

        [HttpGet]
        public async Task<IActionResult> Deallocation()
        {
            var currentUserEmail = User.Identity?.Name;
            var shortProjectNames = await _deallocationRepo.GetShortProjectNamesForUserAsync(currentUserEmail!);
            ViewBag.ShortProjectNames = shortProjectNames;
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetEmployeesByProject(string shortProjectName)
        {
            var employees = await _deallocationRepo.GetEmployeesByProjectAsync(shortProjectName);
            return Json(employees);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDeallocation([FromBody] TicketingTable ticket)
        {
            if (ticket == null || !ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid form data" });
            }

            ticket.RequestedBy = User.Identity?.Name;

            var (success, message) = await _deallocationRepo.SubmitDeallocationAsync(ticket);

            return Json(new { success, message });
        }
    }
}

