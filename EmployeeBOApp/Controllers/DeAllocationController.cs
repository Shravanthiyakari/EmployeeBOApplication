using EmployeeBOApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeBOApp.BussinessLayer.Interfaces;


namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class DeallocationController : Controller
    {
        private readonly IDeallocationService _deallocationService;

        public DeallocationController(IDeallocationService deallocationService)
        {
            _deallocationService = deallocationService;
            
        }

        [HttpGet]
        public async Task<IActionResult> Deallocation()
        {
            var currentUserEmail = User.Identity?.Name;
            var shortProjectNames = await _deallocationService.GetShortProjectNamesForUserAsync(currentUserEmail!);
            ViewBag.ShortProjectNames = shortProjectNames;
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetEmployeesByProject(string shortProjectName)
        {
            var employees = await _deallocationService.GetEmployeesByProjectAsync(shortProjectName);
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

            var (success, message) = await _deallocationService.SubmitDeallocationAsync(ticket);

            return Json(new { success, message });
        }
    }
}

