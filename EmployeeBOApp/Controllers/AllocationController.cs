using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeBOApp.Models;
using EmployeeBOApp.BussinessLayer.Interfaces;

namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class AllocationController : Controller
    {
        private readonly IAllocationService _allocationService;

        public AllocationController(IAllocationService allocationService)
        {
            _allocationService = allocationService;
        }

        public IActionResult Index()
        {
            var currentUserEmail = User.Identity?.Name;
            var shortProjectNames = _allocationService.GetShortProjectNames(currentUserEmail!);
            ViewBag.ShortProjectNames = shortProjectNames;
            return View();
        }

        [HttpGet]
        public JsonResult GetProjectDetails(string shortProjectName)
        {
            var result = _allocationService.GetProjectDetailsByShortName(shortProjectName);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRequest([FromForm] TicketingTable ticket)
        {
            var currentUserEmail = User.Identity?.Name;
            var (success, message) = await _allocationService.SubmitAllocationRequest(ticket, currentUserEmail!);
            return Json(new { success, message });
        }
    }
}
