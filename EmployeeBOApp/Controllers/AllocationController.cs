using Microsoft.AspNetCore.Mvc;
using EmployeeBOApp.Models;
using EmployeeBOApp.EmailsContent;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class AllocationController : Controller
    {
        private readonly EmployeeDatabaseContext _context;
        private readonly IEmailService _emailService;


        public AllocationController(EmployeeDatabaseContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
        public async Task<IActionResult> SubmitRequest([FromForm] TicketingTable ticket)
        {
            // Step 1: Check if there is already an open allocation request
            var existingRequest = await _context.TicketingTables
                .Where(t => t.EmpId == ticket.EmpId &&
                            t.RequestType == "Allocation" &&
                            t.Status != "Closed")
                .FirstOrDefaultAsync();

            if (existingRequest != null)
            {
                return Json(new
                {
                    success = false,
                    message = "An allocation request is already in progress for this employee. Please wait until it's closed before submitting a new one."
                });
            }

            // Step 2: Check if employee exists
            var employee = _context.EmployeeInformations
                .FirstOrDefault(e => e.EmpId == ticket.EmpId);

            if (employee != null && !employee.Deallocation)
            {
                return Json(new
                {
                    success = false,
                    message = "This employee is already allocated. You cannot submit the request again."
                });
            }

            if (employee == null)
            {
                employee = new EmployeeInformation
                {
                    EmpId = ticket?.EmpId,
                    EmpName = ticket.EmpName,
                    ProjectId = ticket.ProjectId,
                    Deallocation = false
                };
                _context.EmployeeInformations.Add(employee);
                _context.SaveChanges();
            }

            if (employee.Deallocation)
            {
                ModelState.AddModelError(string.Empty, "Deallocation process is not yet completed for this employee.");
                return Json(new { success = false, message = "Deallocation process is not yet completed for this employee." });
            }

            // Step 3: Proceed with email + save logic
            var projectInfo = await (from emp in _context.EmployeeInformations
                                     join proj in _context.ProjectInformations
                                         on emp.ProjectId equals proj.ProjectId
                                     where emp.EmpId == ticket.EmpId
                                     select new
                                     {
                                         DmEmail = proj.DmemailId,
                                         proj.ProjectId,
                                         proj.ProjectName
                                     }).FirstOrDefaultAsync();

            var cc = ticket.RequestedBy;
            string dmEmail = projectInfo!.DmEmail;
            string projectName = projectInfo.ProjectName;

            var subject = $"Assignment Request - {employee.EmpName} - {ticket.EmpId}";

            string finalBody = EmailContentforAllocation.EmailContentForDM
                .Replace("{EMP_ID}", ticket.EmpId)
                .Replace("{EMP_NAME}", employee?.EmpName)
                .Replace("{PROJECT_CODE}", employee?.ProjectId)
                .Replace("{START_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                .Replace("{Request_Status}", "Submitted")
                .Replace("{user_name}", User.Identity?.Name);

            try
            {
                await _emailService.SendEmailAsync(
                    toEmails: new List<string> { dmEmail },
                    subject: subject,
                    body: finalBody,
                    isHtml: true,
                    ccEmails: new List<string> { cc }
                );

                ticket.Status = "Open";
                ticket.RequestedDate = DateTime.Now;
                ticket.RequestType = "Allocation";
                _context.TicketingTables.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Allocation saved, but email failed: {ex.Message}" });
            }

            return Json(new { success = true, message = "Allocation submitted and email sent successfully!" });
        }
    }
}