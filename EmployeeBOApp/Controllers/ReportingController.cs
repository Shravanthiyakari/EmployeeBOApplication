using Microsoft.AspNetCore.Mvc;
using EmployeeBOApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class ReportingController : Controller
    {
        private readonly EmployeeDatabaseContext _context;

        public ReportingController(EmployeeDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ReportingManagerChangeRequest()
        {
            var currentUserEmail = User.Identity?.Name;

            var shortProjectNames = _context.ProjectInformations
                .Where(p =>
                    (p.PmemailId == currentUserEmail || p.DmemailId == currentUserEmail) &&
                    p.ShortProjectName != null)
                .Select(p => p.ShortProjectName)
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

            if (project != null)
            {
                return Json(new
                {
                    projectId = project.ProjectId,
                    projectName = project.ProjectName,
                    reportingManager = project.Pm,
                    projectManager = project.Pm // Adjust if there's a separate field for PM
                });
            }

            return Json(null);
        }

        [HttpPost]
        public IActionResult SubmitRequest(TicketingTable model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var ticket = new TicketingTable
                    {
                        EmpId = model.EmpId,
                        RequestedBy = model.RequestedBy,
                        RequestedDate = model.RequestedDate,
                        Status = "Pending",
                        Comments = model.Comments,
                        EndDate = model.EndDate
                    };

                

                    ticket.RequestedDate = DateTime.Now;
                    _context.TicketingTables.Add(ticket);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Manager Change Request submitted successfully!";
                    return RedirectToAction("ReportingManagerChangeRequest");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to database: {ex.Message}");
            }

            return View("ReportingManagerChangeRequest", model);
        }
    }

    }