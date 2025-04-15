using Microsoft.AspNetCore.Mvc;
using EmployeeBOApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeBOApp.Controllers
{
    public class DeallocationController : Controller
    {
        private readonly EmployeeDatabaseContext _context;

        public DeallocationController(EmployeeDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Deallocation()
        {
            var currentUserEmail = User.Identity?.Name;

            var shortProjectNames = _context.ProjectInformations
                .Where(p =>
                    (p.PmemailId == currentUserEmail || p.DmemailId == currentUserEmail) &&
                    p.ShortProjectName != null)
                .Select(p => p.ShortProjectName!)
                .Distinct()
                .ToList();

            ViewBag.ShortProjectNames = shortProjectNames;
            return View();
        }

        [HttpGet]
        public JsonResult GetEmployeesByProject(string shortProjectName)
        {
            var employees = _context.EmployeeInformations
                .Include(e => e.Project)
                .Where(e => e.Project != null && e.Project.ShortProjectName == shortProjectName)
                .Select(e => new
                {
                    e.EmpName,
                    e.EmpId,
                    e.ProjectId,
                    ProjectName = e.Project!.ProjectName,
                    ProjectManager = e.Project.Pm,
                    DeliveryManager = e.Project.Dm,
                    RequestedBy = e.Project.PmemailId
                }).ToList();

            return Json(employees);
        }

        [HttpPost]
        public IActionResult SubmitDeallocation([FromBody] TicketingTable ticket)
        {
            if (ticket == null || !ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid form data" });
            }
            ticket.RequestType = "Deallocation";
            ticket.EndDate = DateTime.Now;
            ticket.RequestedDate = DateTime.Now;


            _context.TicketingTables.Add(ticket);
            _context.SaveChanges();

            return Json(new { success = true, message = "Deallocation submitted successfully!" });
        }
    }
}
