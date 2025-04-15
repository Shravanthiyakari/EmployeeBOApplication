using Microsoft.AspNetCore.Mvc;
using EmployeeBOApp.Models;
using System.Linq;

namespace EmployeeBOApp.Controllers
{
    public class AllocationController : Controller
    {
        private readonly EmployeeDatabaseContext _context;

        public AllocationController(EmployeeDatabaseContext context)
        {
            _context = context;
        }

        public IActionResult Index()
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
        public JsonResult GetProjectDetails(string shortProjectName)
        {
            var project = _context.ProjectInformations
                .FirstOrDefault(p => p.ShortProjectName == shortProjectName);

            if (project == null)
            {
                return Json(new { success = false, message = "Project not found" });
            }

            return Json(new
            {
                success = true,
                projectCode = project.ProjectId,
                projectName = project.ProjectName,
                projectManager = project.Pm,
                deliveryManager = project.Dm,
                pmEmail = project.PmemailId,
                dmEmail = project.DmemailId
            });
        }

        [HttpPost]
        public IActionResult SubmitRequest([FromForm] TicketingTable ticket)
        {
            var employee = _context.EmployeeInformations
                .FirstOrDefault(e => e.EmpId == ticket.EmpId && e.Deallocation == true);

            if (employee == null)
            {
                ModelState.AddModelError(string.Empty, "Employee is not marked as deallocated,already allocated.");
            }

            if (ModelState.IsValid)
            {
                ticket.RequestedDate = DateTime.Now;
                ticket.RequestType = "Allocation";
                _context.TicketingTables.Add(ticket);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Allocation Request submitted successfully!";
                return RedirectToAction("Index");
            }

            // Reload dropdown list if model state is invalid
            var currentUserEmail = User.Identity?.Name;
            ViewBag.ShortProjectNames = _context.ProjectInformations
                .Where(p =>
                    (p.PmemailId == currentUserEmail || p.DmemailId == currentUserEmail) &&
                    p.ShortProjectName != null)
                .Select(p => p.ShortProjectName!)
                .Distinct()
                .ToList();

            return View("Index", ticket);
        }
    }
    }
