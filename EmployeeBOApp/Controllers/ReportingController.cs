using EmployeeBOApp.BussinessLayer.Implementations;
using EmployeeBOApp.BussinessLayer.Interfaces;
using EmployeeBOApp.Data;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class ReportingController : Controller
    {
        private readonly EmployeeDatabaseContext _context;
        private readonly IReportingService _repo;

        public ReportingController(EmployeeDatabaseContext context, IReportingService repo)
        {
            _context = context;
            _repo = repo;
        }

        [HttpGet]
        public IActionResult ReportingManagerChangeRequest()
        {
            var currentUserEmail = User.Identity?.Name;

            var shortProjectNames = _repo.GetShortProjectNames(currentUserEmail!);

            ViewBag.ShortProjectNames = shortProjectNames;
            return View();
        }

        [HttpGet]
        public JsonResult GetProjectDetails(string shortProjectName)
        {
            var project = _repo.GetProjectDetails(shortProjectName);

            if (project != null)
            {
                return Json(new
                {
                    projectId = project.ProjectId,
                    projectName = project.ProjectName,
                    departmentId = project.DepartmentID,
                    reportingManager = project.Pm,
                    projectManager = project.Pm
                });
            }

            return Json(null);
        }

        [HttpGet]
        public JsonResult GetEmployeesByProject(string shortProjectName)
        {
            var employees = _repo.GetEmployeesByProject(shortProjectName);
            return Json(employees);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRequest([FromForm] TicketingTable ticket)
        {
            if (ticket == null || !ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid form data" });
            }

            // List of mutually exclusive types
            var exclusiveTypes = new[] { "Reporting Change", "Manager Change", "Department Change" };

            // Check only if current request is one of the exclusive types
            if (exclusiveTypes.Contains(ticket.RequestType))
            {
                // Check for conflicting active requests among the other two types
                var conflictingRequest = await _repo.CheckForConflictingExclusiveRequestAsync(ticket, exclusiveTypes);

                //var conflictingRequest = await _context.TicketingTables
                //    .Where(t => t.EmpId == ticket.EmpId
                //                && t.RequestType != ticket.RequestType
                //                && exclusiveTypes.Contains(t.RequestType)
                //                && (t.Status == "Open" || t.Status == "InProgress"))
                //    .FirstOrDefaultAsync();

                if (conflictingRequest != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"An active '{conflictingRequest.RequestType}' request already exists for this employee. Please close it before submitting a '{ticket.RequestType}' request."
                    });
                }

                // Prevent duplicate active submission of the same type

                var duplicateRequest = await _repo.CheckForDuplicateRequestAsync(ticket);

                //var duplicateRequest = await _context.TicketingTables
                //    .Where(t => t.EmpId == ticket.EmpId
                //                && t.RequestType == ticket.RequestType
                //                && (t.Status == "Open" || t.Status == "InProgress"))
                //    .FirstOrDefaultAsync();

                if (duplicateRequest != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"A '{ticket.RequestType}' request is already Open or InProgress for this employee."
                    });
                }
            }

            var employee = await _repo.GetEmployeeByIdAsync(ticket.EmpId!);
            var projectInfo = await _repo.GetProjectInfoByEmployeeIdAsync(ticket.EmpId!);
            string requestedByEmail = User.Identity?.Name!;

            ticket.Status = "Open";
            ticket.RequestedDate = DateTime.Now;
            ticket.EndDate = DateTime.Now;
            ticket.RequestedBy = requestedByEmail;

            var emailResult = await _repo.SendReportingChangeEmailAsync(ticket, employee, projectInfo, requestedByEmail);
            await _repo.SaveTicketAsync(ticket, employee);

            return Json(new
            {
                success = emailResult.Success,
                message = emailResult.Success
                    ? "Data submitted and email sent successfully!"
                    : $"Data saved, but email failed: {emailResult.Message}"
            });
        }
    }
}