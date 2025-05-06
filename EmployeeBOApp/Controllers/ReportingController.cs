using Microsoft.AspNetCore.Mvc;
using EmployeeBOApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using EmployeeBOApp.EmailsContent;


namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class ReportingController : Controller
    {
        private readonly EmployeeDatabaseContext _context;
        private readonly IEmailService _emailService;


        public ReportingController(EmployeeDatabaseContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
            var project = _context.ProjectInformations
                .FirstOrDefault(p => p.ShortProjectName == shortProjectName);

            if (project != null)
            {
                var employees = _context.EmployeeInformations
                    .Where(e => e.ProjectId == project.ProjectId)
                    .Select(e => new
                    {
                        empId = e.EmpId,
                        empName = e.EmpName

                    })
                    .ToList();

                return Json(employees);
            }

            return Json(new List<object>());
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRequest([FromForm]TicketingTable ticket)
        {
            
            if (ticket == null || !ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid form data" });
            }
            var employee = await _context.EmployeeInformations.FirstOrDefaultAsync(e => e.EmpId == ticket.EmpId);

            //var existingRequest = await _context.TicketingTables.Where(t => t.EmpId == ticket.EmpId &&
            //   t.RequestType == "Reporting Manager" &&
            //   (t.Status == "InProgress" || t.Status == "Open")).FirstOrDefaultAsync();
            //if (existingRequest != null)
            //{
            //    // If an existing request is found, prevent creating a new one
            //    return Json(new { success = false, message = "A request for reporting/manager/department change is already in progress or open for this employee." });
            //}


            var projectInfo = await (from emp in _context.EmployeeInformations
                                     join proj in _context.ProjectInformations
                                         on emp.ProjectId equals proj.ProjectId
                                     where emp.EmpId == ticket.EmpId
                                     select new
                                     {
                                         DmEmail = proj.DmemailId,
                                         proj.ProjectName,
                                         departmentId = proj.DepartmentID,
                                         proj.Dm,

                                     }).FirstOrDefaultAsync();

            string dmEmail = projectInfo!.DmEmail;
            string projectName = projectInfo.ProjectName;


            var subject = $" {ticket.RequestType} Request - {ticket.EmpId} - {employee.EmpName} ";

            var cc = User.Identity?.Name;
            // 2. Replace placeholders with real values
            string finalBody = EmailContentforRDRequestChange.EmailContentForRDC
                .Replace("{EMP_ID}", ticket.EmpId)
                .Replace("{EMP_NAME}", employee?.EmpName)
                .Replace("{PROJECT_CODE}", employee?.ProjectId)
                .Replace("{PROJECT_Name}", projectName)
                .Replace("{DEPARTMENT_ID}", projectInfo.departmentId)
                .Replace("{ReportingManager}",projectInfo.Dm)
                .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                .Replace("{Request_Status}", "Submitted")
                .Replace("{user_name}", User.Identity?.Name);

            try
            {
                // 3. Send HTML email
                await _emailService.SendEmailAsync(
                    toEmails: new List<string> { dmEmail },
                    subject: subject,
                    body: finalBody,
                    isHtml: true,
                    ccEmails: new List<string> { cc }
                );

                // 4. Save to DB
                ticket.Status = "Open";
                ticket.RequestedDate = DateTime.Now;
                ticket.RequestType =ticket.RequestType;
                ticket.EndDate = DateTime.Now;
                ticket.ApprovedBy = ticket.ApprovedBy;
                ticket.RequestedBy = User.Identity?.Name;
                //ticket.StartDate
                ticket.ApprovedDate= ticket.ApprovedDate;
                ticket.DepartmentID = ticket.DepartmentID;
               
                _context.TicketingTables.Add(ticket);
                _context.EmployeeInformations.Update(employee!);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Data saved, but email failed: {ex.Message}" });
            }

            return Json(new { success = true, message = "Data submitted and email sent successfully!" });
        }
    }
}