using Microsoft.AspNetCore.Mvc;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using EmployeeBOApp.Data;

namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class ReportingController : Controller
    {
        private readonly EmployeeDatabaseContext _context;
        private readonly IReportingRepository _repo;

        public ReportingController(EmployeeDatabaseContext context, IReportingRepository repo)
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

            var employee = await _repo.GetEmployeeByIdAsync(ticket.EmpId!);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            var projectInfo = await _repo.GetProjectInfoByEmployeeIdAsync(ticket.EmpId!);
            string requestedByEmail = User.Identity?.Name!;
            ticket.Status = "Open";
            ticket.RequestedDate = DateTime.Now;
            ticket.EndDate = DateTime.Now;
            ticket.RequestedBy = requestedByEmail;

            var emailResult = await _repo.SendReportingChangeEmailAsync(ticket, employee, projectInfo, requestedByEmail!);

            // Save the ticket after attempting to send email
            

            await _repo.SaveTicketAsync(ticket, employee);

            return Json(new
            {
                success = emailResult.Success,
                message = emailResult.Success
                    ? "Data submitted and email sent successfully!"
                    : $"Data saved, but {emailResult.Message}"
            });
        }
    }
}