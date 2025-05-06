using ClosedXML.Excel;
using EmployeeBOApp.EmailsContent;
using EmployeeBOApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Security.Claims;

namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class ViewController : Controller
    {
        private readonly EmployeeDatabaseContext _context;
        private readonly IEmailService _emailService;


        public ViewController(EmployeeDatabaseContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string searchQuery, string requestType, int page = 1)
        {
            var userEmail = User.Identity?.Name;
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            var ticketsQuery = _context.TicketingTables
                .Include(t => t.Emp)
                    .ThenInclude(e => e!.Project)
                .AsQueryable();

            // Role-based filtering
            if (userRoles.Contains("GDO"))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Status == "InProgress" || t.Status == "Closed" || t.Status == "Reject");
            }
            else if (userRoles.Contains("DM"))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Emp!.Project!.DmemailId == userEmail);
            }
            else if (userRoles.Contains("PM"))
            {
                ticketsQuery = ticketsQuery.Where(t => t.RequestedBy == userEmail);
            }

            // Apply RequestType filtering (if not "Select")
            if (!string.IsNullOrEmpty(requestType) && requestType != "Select")
            {
                ticketsQuery = ticketsQuery.Where(t => t.RequestType == requestType);
            }

            // Apply search query
            if (!string.IsNullOrEmpty(searchQuery))
            {
                ticketsQuery = ticketsQuery.Where(t =>
                    t.Emp!.EmpId.Contains(searchQuery) ||
                    t.Emp.EmpName.Contains(searchQuery) ||
                    t.Emp.Project!.ShortProjectName!.Contains(searchQuery) ||
                    t.RequestType!.Contains(searchQuery) ||
                    t.Emp.Project.ProjectName.Contains(searchQuery) ||
                    t.Emp.Project.Pm!.Contains(searchQuery) ||
                    t.Status!.Contains(searchQuery)
                );
            }

            // Paging logic
            int pageSize = 10;
            int totalItems = await ticketsQuery.CountAsync();
            var pagedResult = await ticketsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["SearchQuery"] = searchQuery;
            ViewData["RequestType"] = requestType;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(pagedResult);
        }

        
        [HttpPost]
        public IActionResult ExportToExcel(string searchQuery, string requestType)
        {
            // Retrieve data based on searchQuery, requestType, and role-based filtering
            var userEmail = User.Identity?.Name;
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            var ticketsQuery = _context.TicketingTables
                .Include(t => t.Emp)
                .ThenInclude(e => e!.Project)
                .AsQueryable();

            // Apply search query filtering if provided
            if (!string.IsNullOrEmpty(searchQuery) && searchQuery != "All")
            {
                ticketsQuery = ticketsQuery.Where(t =>
                    t.Emp!.EmpId.Contains(searchQuery) ||
                    t.Emp.EmpName.Contains(searchQuery) ||
                    t.Emp.Project!.ShortProjectName!.Contains(searchQuery) ||
                    t.RequestType!.Contains(searchQuery) ||
                    t.Emp.Project.ProjectName.Contains(searchQuery) ||
                    t.Emp.Project.Pm!.Contains(searchQuery) ||
                    t.Status!.Contains(searchQuery)
                );
            }

            // Apply requestType filtering if not "All"
            if (!string.IsNullOrEmpty(requestType) && requestType != "Select" && requestType != "All")
            {
                ticketsQuery = ticketsQuery.Where(t => t.RequestType == requestType);
            }

            var result = ticketsQuery.ToList();

            // Create an Excel file using ClosedXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Requests");

                // Define headers
                worksheet.Cell(1, 1).Value = "Emp ID";
                worksheet.Cell(1, 2).Value = "Emp Name";
                worksheet.Cell(1, 3).Value = "Request Type";
                worksheet.Cell(1, 4).Value = "Project Code";
                worksheet.Cell(1, 5).Value = "Project Name";
                worksheet.Cell(1, 6).Value = "PM";
                worksheet.Cell(1, 7).Value = "Status";

                // Populate rows with data
                for (int i = 0; i < result.Count; i++)
                {
                    var ticket = result[i];
                    var emp = ticket.Emp;
                    var project = emp?.Project;

                    worksheet.Cell(i + 2, 1).Value = emp?.EmpId;
                    worksheet.Cell(i + 2, 2).Value = emp?.EmpName;
                    worksheet.Cell(i + 2, 3).Value = ticket.RequestType;
                    worksheet.Cell(i + 2, 4).Value = project?.ShortProjectName;
                    worksheet.Cell(i + 2, 5).Value = project?.ProjectName;
                    worksheet.Cell(i + 2, 6).Value = project?.Pm;
                    worksheet.Cell(i + 2, 7).Value = ticket.Status;
                }

                // Set the content type and file name for the response
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var fileName = "ViewRequests.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> CloseRequest(int id)
        {
            var ticket = await _context.TicketingTables.FindAsync(id);
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            if (ticket != null && ticket.Status == "InProgress" && userRoles.Contains("GDO"))
            {
                ticket.Status = "Closed";
                ticket.ApprovedDate = DateTime.Now;
                ticket.ApprovedBy = User.Identity?.Name;
                await _context.SaveChangesAsync();
                var gdoEmailIds = await _context.Logins.Where(login => login.Role == "GDO").Select(login => login.EmailId).ToListAsync();
                var employee = await _context.EmployeeInformations.FirstOrDefaultAsync(e => e.EmpId == ticket.EmpId);

                var subject = $" {ticket.RequestType} Ticket Status Details - {ticket.EmpId} - {employee?.EmpName} - ";
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
                string finalBody;
                if (ticket.RequestType == "ReportingChange" || ticket!.RequestType == "ManagerChange" || ticket.RequestType == "DepartmentChange")
                {
                    // 2. Replace placeholders with real values
                    finalBody = EmailContentforRDRequestChange.EmailContentForRDC
                        .Replace("{EMP_ID}", ticket.EmpId)
                        .Replace("{EMP_NAME}", employee?.EmpName)
                        .Replace("{PROJECT_CODE}", employee?.ProjectId)
                        .Replace("{PROJECT_Name}", projectName)
                        .Replace("{DEPARTMENT_ID}", projectInfo.departmentId)
                        .Replace("{ReportingManager}", projectInfo.Dm)
                        .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                        .Replace("{Request_Status}", "Closed")
                        .Replace("{user_name}", User.Identity?.Name);
                }
                else
                {

                     finalBody = EmailContentforDeallocation.EmailContentForDM
                .Replace("{EMP_ID}", ticket.EmpId)
                    .Replace("{EMP_NAME}", employee?.EmpName)
                    .Replace("{PROJECT_CODE}", employee?.ProjectId)
                    .Replace("{END_DATE}", ticket.EndDate.ToString())
                    .Replace("{Request_Status}", "Closed")
                    .Replace("{user_name}", User.Identity?.Name);
                }

                    try
                    {

                        await _emailService.SendEmailAsync(
                                          toEmails: gdoEmailIds,
                                          subject: subject,
                                          body: finalBody,
                                          isHtml: true,
                                          ccEmails: new List<string> { projectInfo.DmEmail, ticket.RequestedBy }
                                          );
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = $"Approval of the ticket is failed, {ex.Message}" });
                    }
                    return RedirectToAction("Index");
                }
            
            else
            {
                // Add a return statement for the case where the approval criteria are not met
                return BadRequest("Invalid request for approval."); // Or NotFound(), RedirectToAction(), etc.
            }
            // Email notification logic can be added here
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var ticket = await _context.TicketingTables.FindAsync(id);
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            if (ticket != null && ticket.Status == "Open" && userRoles.Contains("DM"))
            {
                ticket.Status = "InProgress";
                ticket.ApprovedDate = DateTime.Now;
                ticket.ApprovedBy = User.Identity?.Name;
                await _context.SaveChangesAsync();
                var gdoEmailIds = await _context.Logins
                    .Where(login => login.Role == "GDO")
                    .Select(login => login.EmailId)
                .ToListAsync();
                var employee = await _context.EmployeeInformations.FirstOrDefaultAsync(e => e.EmpId == ticket.EmpId);

                var subject = $" {ticket.RequestType} Ticket Status Details - {ticket.EmpId} - {employee?.EmpName} - ";
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
                string finalBody;
                if (ticket.RequestType == "ReportingChange" || ticket!.RequestType == "ManagerChange" || ticket.RequestType == "DepartmentChange")
                {
                    finalBody = EmailContentforRDRequestChange.EmailContentForRDC

                        .Replace("{EMP_ID}", ticket.EmpId)
                        .Replace("{EMP_NAME}", employee?.EmpName)
                        .Replace("{PROJECT_CODE}", employee?.ProjectId)
                        .Replace("{PROJECT_Name}", projectName)
                        .Replace("{DEPARTMENT_ID}", projectInfo.departmentId)
                        .Replace("{ReportingManager}", projectInfo.Dm)
                        .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                        .Replace("{Request_Status}", "Approved")
                        .Replace("{user_name}", User.Identity?.Name);
                }
                else
                {

                    finalBody = EmailContentforDeallocation.EmailContentForDM
                    .Replace("{EMP_ID}", ticket.EmpId)
                   .Replace("{EMP_NAME}", employee?.EmpName)
                   .Replace("{PROJECT_CODE}", employee?.ProjectId)
                   .Replace("{END_DATE}", ticket.EndDate.ToString())
                   .Replace("{Request_Status}", "Approved")
                   .Replace("{user_name}", User.Identity?.Name);
                }

                try
                {

                    await _emailService.SendEmailAsync(
                                      toEmails: gdoEmailIds,
                                      subject: subject,
                                      body: finalBody,
                                      isHtml: true,
                                      ccEmails: new List<string> { User.Identity?.Name, ticket.RequestedBy }
                                      );
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Approval of the ticket is failed, {ex.Message}" });
                }
                return RedirectToAction("Index");
            }
            else
            {
                // Add a return statement for the case where the approval criteria are not met
                return BadRequest("Invalid request for approval."); // Or NotFound(), RedirectToAction(), etc.
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var ticket = await _context.TicketingTables.FindAsync(id);
            if (ticket != null && ticket.Status == "Open" && ticket.RequestedBy == User.Identity?.Name)
            {
                var employee = await _context.EmployeeInformations.FirstOrDefaultAsync(e => e.EmpId == ticket.EmpId);

                _context.TicketingTables.Remove(ticket);
                //ticket.Status = "Deleted";
                //ticket.ApprovedBy = User.Identity?.Name;
                //ticket.ApprovedDate = DateTime.Now;
                employee!.Deallocation = false;
                _context.EmployeeInformations.Update(employee);
                await _context.SaveChangesAsync();

                var gdoEmailIds = await _context.Logins
                    .Where(login => login.Role == "GDO")
                    .Select(login => login.EmailId)
                    .ToListAsync();

                var subject = $" {ticket.RequestType} Ticket Status Details - {ticket.EmpId} - {employee?.EmpName} - ";
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
                string finalBody;
                if (ticket.RequestType == "ReportingChange" || ticket!.RequestType == "ManagerChange"|| ticket.RequestType == "DepartmentChange")
                {
                    finalBody = EmailContentforRDRequestChange.EmailContentForRDC

                        .Replace("{EMP_ID}", ticket.EmpId)
                        .Replace("{EMP_NAME}", employee?.EmpName)
                        .Replace("{PROJECT_CODE}", employee?.ProjectId)
                        .Replace("{PROJECT_Name}", projectName)
                        .Replace("{DEPARTMENT_ID}", projectInfo.departmentId)
                        .Replace("{ReportingManager}", projectInfo.Dm)
                        .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                        .Replace("{Request_Status}", "Deleted")
                        .Replace("{user_name}", User.Identity?.Name);
                }
                else
                {
                 finalBody = EmailContentforDeallocation.EmailContentForDM
                .Replace("{EMP_ID}", ticket.EmpId)
                .Replace("{EMP_NAME}", employee?.EmpName)
                .Replace("{PROJECT_CODE}", employee?.ProjectId)
                .Replace("{END_DATE}", ticket.EndDate.ToString())
                .Replace("{Request_Status}", "Deleted")
                .Replace("{user_name}", User.Identity?.Name);
                }

                try
                {

                    await _emailService.SendEmailAsync(
                                      toEmails: gdoEmailIds,
                                      subject: subject,
                                      body: finalBody,
                                      isHtml: true,
                                      ccEmails: new List<string> { User.Identity?.Name, ticket.RequestedBy }
                                      );
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Approval of the ticket is failed, {ex.Message}" });
                }

                return RedirectToAction("Index");
            }
            else
            {
                // Add a return statement for the case where the approval criteria are not met
                return BadRequest("Invalid request for approval."); // Or NotFound(), RedirectToAction(), etc.
            }
        }


        [HttpPost]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var ticket = await _context.TicketingTables.FindAsync(id);
            var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

            if (ticket != null && (ticket.Status == "InProgress" || ticket.Status == "Open" )&& (userRoles.Contains("GDO") || userRoles.Contains("DM")))
            {
                ticket.Status = "Rejected";
                ticket.ApprovedBy = User.Identity?.Name;
                ticket.ApprovedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                var gdoEmailIds = await _context.Logins
                               .Where(login => login.Role == "GDO")
                               .Select(login => login.EmailId)
                               .ToListAsync();
                var employee = await _context.EmployeeInformations.FirstOrDefaultAsync(e => e.EmpId == ticket.EmpId);
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
                var subject = $" {ticket.RequestType} Ticket Status Details - {ticket.EmpId} - {employee?.EmpName} - ";
                string finalBody;
                if (ticket.RequestType == "ReportingChange" || ticket!.RequestType == "ManagerChange" || ticket.RequestType == "DepartmentChange")
                {
                    finalBody = EmailContentforRDRequestChange.EmailContentForRDC

                        .Replace("{EMP_ID}", ticket.EmpId)
                        .Replace("{EMP_NAME}", employee?.EmpName)
                        .Replace("{PROJECT_CODE}", employee?.ProjectId)
                        .Replace("{PROJECT_Name}", projectName)
                        .Replace("{DEPARTMENT_ID}", projectInfo.departmentId)
                        .Replace("{ReportingManager}", projectInfo.Dm)
                        .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                        .Replace("{Request_Status}", "Rejected")
                        .Replace("{user_name}", User.Identity?.Name);
                }
                else
                {
                    finalBody = EmailContentforDeallocation.EmailContentForDM
                   .Replace("{EMP_ID}", ticket.EmpId)
                   .Replace("{EMP_NAME}", employee?.EmpName)
                   .Replace("{PROJECT_CODE}", employee?.ProjectId)
                   .Replace("{END_DATE}", ticket.EndDate.ToString())
                   .Replace("{Request_Status}", "Rejected")
                   .Replace("{user_name}", User.Identity?.Name);
                }

                try
                {

                    await _emailService.SendEmailAsync(
                                      toEmails: gdoEmailIds,
                                      subject: subject,
                                      body: finalBody,
                                      isHtml: true,
                                      ccEmails: new List<string> { User.Identity?.Name, ticket.RequestedBy }
                                      );
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Rejection of the ticket is failed, {ex.Message}" });
                }
                return RedirectToAction("Index");
            }
            else
            {
                
                return BadRequest("Invalid request for approval."); 
            }
        }

    }
}