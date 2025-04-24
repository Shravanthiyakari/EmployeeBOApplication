using Microsoft.AspNetCore.Mvc;
using EmployeeBOApp.Models;
using Microsoft.EntityFrameworkCore;
using EmployeeBOApp.EmailsContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class DeallocationController : Controller
    {
        private readonly EmployeeDatabaseContext _context;
        private readonly IEmailService _emailService;

        public DeallocationController(EmployeeDatabaseContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
        public async Task<IActionResult> SubmitDeallocation([FromBody] TicketingTable ticket)
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
                    message = "An allocation request is already in progress for this employee. Please wait until it's closed."
                });
            }
            if (ticket == null || !ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid form data" });
            }

            var employee = await _context.EmployeeInformations.FirstOrDefaultAsync(e => e.EmpId == ticket.EmpId);

            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found." });
            }

            if (employee.Deallocation == true)
            {
                return Json(new { success = false, message = "Deallocation is already submitted." });
            }

            var projectInfo = await (from emp in _context.EmployeeInformations
                                     join proj in _context.ProjectInformations
                                         on emp.ProjectId equals proj.ProjectId
                                     where emp.EmpId == ticket.EmpId
                                     select new
                                     {
                                         DmEmail = proj.DmemailId,
                                         proj.ProjectName
                                     }).FirstOrDefaultAsync();

            if (projectInfo == null)
            {
                return Json(new { success = false, message = "Project information not found." });
            }

            string dmEmail = projectInfo.DmEmail;
            string projectName = projectInfo.ProjectName;

            string subject = $"Deallocation Request - {projectName} - {ticket.EmpId} - {employee.EmpName}";

            string finalBody = EmailContentforDeallocation.EmailContentForDM
                .Replace("{EMP_ID}", ticket.EmpId)
                .Replace("{EMP_NAME}", employee.EmpName)
                .Replace("{PROJECT_CODE}", employee.ProjectId)
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
                    ccEmails: new List<string> { ticket.RequestedBy! }
                );

                ticket.Status = "Open";
                ticket.RequestedDate = DateTime.Now;
                ticket.RequestType = "Deallocation";

                _context.TicketingTables.Add(ticket);

                employee.Deallocation = true;
                _context.EmployeeInformations.Update(employee);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Deallocation submitted and email sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Deallocation saved, but email failed: {ex.Message}" });
            }
        }
    }
}
