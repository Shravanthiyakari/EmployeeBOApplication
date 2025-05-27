using EmployeeBOApp.Data;
using EmployeeBOApp.EmailsContent;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeBOApp.Repositories
{
    public class ViewRepository : IViewRepository
    {
        private readonly EmployeeDatabaseContext _context;
        private readonly IEmailService _emailService;
        private string actionLink = "";

        public ViewRepository(EmployeeDatabaseContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<(List<TicketingTable> Tickets, int TotalCount)> GetFilteredTicketsAsync(
          string? searchQuery,
          string? requestType,
          string? userEmail,
          List<string> userRoles,
          int page,
          int pageSize = 10)
        {
            var query = _context.TicketingTables
                .Include(t => t.Emp)
                    .ThenInclude(e => e!.Project)
                .AsQueryable();

            // Role-based filtering
            if (userRoles.Contains("GDO"))
            {
                query = query.Where(t => t.Status == "InProgress" || t.Status == "Closed" || t.Status == "Rejected");
            }
            else if (userRoles.Contains("DM"))
            {
                query = query.Where(t => t.Emp!.Project!.DmemailId == userEmail);
            }
            else if (userRoles.Contains("PM"))
            {
                query = query.Where(t => t.RequestedBy == userEmail);
            }

            // RequestType filter
            if (!string.IsNullOrEmpty(requestType) && requestType != "Select")
            {
                query = query.Where(t => t.RequestType == requestType);
            }

            // Search filter
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(t =>
                    t.Emp!.EmpId.Contains(searchQuery) ||
                    t.Emp.EmpName.Contains(searchQuery) ||
                    t.Emp.Project!.ShortProjectName!.Contains(searchQuery) ||
                    t.RequestType!.Contains(searchQuery) ||
                    t.Emp.Project.ProjectName.Contains(searchQuery) ||
                    t.Emp.Project.Pm!.Contains(searchQuery) ||
                    t.Status!.Contains(searchQuery)
                );
            }

            int totalItems = await query.CountAsync();

            var tickets = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (tickets, totalItems);
        }

        public async Task<List<TicketingTable>> GetTicketsForExportAsync(
         string? searchQuery,
         string? requestType,
         string? userEmail,
         List<string> userRoles)
        {
            var query = _context.TicketingTables
                .Include(t => t.Emp)
                    .ThenInclude(e => e!.Project)
                .AsQueryable();

            // Role-based filtering
            if (userRoles.Contains("GDO"))
            {
                query = query.Where(t => t.Status == "InProgress" || t.Status == "Closed" || t.Status == "Rejected");
            }
            else if (userRoles.Contains("DM"))
            {
                query = query.Where(t => t.Emp!.Project!.DmemailId == userEmail);
            }
            else if (userRoles.Contains("PM"))
            {
                query = query.Where(t => t.RequestedBy == userEmail);
            }

            // Search filter
            if (!string.IsNullOrEmpty(searchQuery) && searchQuery != "All")
            {
                query = query.Where(t =>
                    t.Emp!.EmpId.Contains(searchQuery) ||
                    t.Emp.EmpName.Contains(searchQuery) ||
                    t.Emp.Project!.ShortProjectName!.Contains(searchQuery) ||
                    t.RequestType!.Contains(searchQuery) ||
                    t.Emp.Project.ProjectName.Contains(searchQuery) ||
                    t.Emp.Project.Pm!.Contains(searchQuery) ||
                    t.Status!.Contains(searchQuery)
                );
            }

            // RequestType filter
            if (!string.IsNullOrEmpty(requestType) && requestType != "Select" && requestType != "All")
            {
                query = query.Where(t => t.RequestType == requestType);
            }

            return await query.ToListAsync();
        }
        public async Task<(bool Success, string? ErrorMessage)> CloseRequestAsync(int ticketId, string approvedBy, List<string> userRoles)
        {
            var ticket = await _context.TicketingTables.FindAsync(ticketId);

            if (ticket == null || ticket.Status != "InProgress" || !userRoles.Contains("GDO"))
            {
                return (false, "Invalid request for approval.");
            }

            ticket.Status = "Closed";
            ticket.ApprovedDate = DateTime.Now;
            ticket.ApprovedBy = approvedBy;
            ticket.StartDate= DateTime.Now;
           
            if (ticket.Status != "Closed")
            {
                actionLink = "< a class='action-link' href='https://localhost:7168/Login/Login'>Click here to take decision</a>";
            }

            var gdoEmailIds = await _context.Logins
                .Where(login => login.Role == "GDO")
                .Select(login => login.EmailId)
                .ToListAsync();

            var employee = await _context.EmployeeInformations
                .FirstOrDefaultAsync(e => e.EmpId == ticket.EmpId);

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

            if (employee == null || projectInfo == null)
            {
                return (false, "Employee or Project info not found.");
            }

            var subject = $" {ticket.RequestType} Ticket Status Details - {ticket.EmpId} - {employee.EmpName} - ";

            string finalBody;

            if (ticket.RequestType is "ReportingChange" or "ManagerChange" or "DepartmentChange")
            {
                finalBody = EmailContentforRDRequestChange.EmailContentForRDC
                    .Replace("{EMP_ID}", ticket.EmpId)
                    .Replace("{EMP_NAME}", employee.EmpName)
                    .Replace("{PROJECT_CODE}", employee.ProjectId)
                    .Replace("{PROJECT_Name}", projectInfo.ProjectName)
                    .Replace("{DEPARTMENT_ID}", projectInfo.departmentId)
                    .Replace("{ReportingManager}", projectInfo.Dm)
                    .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                    .Replace("{Request_Status}", "Closed")
                    .Replace("{user_name}", approvedBy)
                    .Replace("{ACTION_LINK}", actionLink);

            }
            else if (ticket.RequestType is "Deallocation")
            {
                finalBody = EmailContentforDeallocation.EmailContentForDM
                    .Replace("{EMP_ID}", ticket.EmpId)
                    .Replace("{EMP_NAME}", employee.EmpName)
                    .Replace("{PROJECT_CODE}", employee.ProjectId)
                    .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                    .Replace("{Request_Status}", "Closed")
                    .Replace("{user_name}", approvedBy)
                    .Replace("{ACTION_LINK}", actionLink);
            }
            else
            {
                finalBody = EmailContentforAllocation.EmailContentForAM
                .Replace("{EMP_ID}", ticket.EmpId)
                .Replace("{EMP_NAME}", employee.EmpName)
                .Replace("{PROJECT_CODE}", employee.ProjectId)
                .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                .Replace("{Request_Status}", "Closed")
                .Replace("{user_name}", approvedBy)
                .Replace("{ACTION_LINK}", actionLink);
            }

            try
            {
                await _emailService.SendEmailAsync(
                    toEmails: gdoEmailIds,
                    subject: subject,
                    body: finalBody,
                    isHtml: true,
                    ccEmails: new List<string> { projectInfo.DmEmail, ticket.RequestedBy! });

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Approval of the ticket failed: {ex.Message}");
            }
        }




        public async Task<(bool Success, string? ErrorMessage)> ApproveRequestAsync(int ticketId, string approvedBy, List<string> userRoles)
        {
            var ticket = await _context.TicketingTables.FindAsync(ticketId);

            if (ticket == null || ticket.Status != "Open" || !userRoles.Contains("DM"))
            {
                return (false, "Invalid request for approval.");
            }

            ticket.Status = "InProgress";
            ticket.ApprovedDate = DateTime.Now;
            ticket.ApprovedBy = approvedBy;
           ticket.StartDate = DateTime.Now;
            if (ticket.Status == "InProgress")
            {
                actionLink = "<a class='action-link' href='https://localhost:7168/Login/Login'>Click here to take decision</a>";
            }

            var gdoEmailIds = await _context.Logins
                .Where(login => login.Role == "GDO")
                .Select(login => login.EmailId)
                .ToListAsync();

            var employee = await _context.EmployeeInformations
                .FirstOrDefaultAsync(e => e.EmpId == ticket.EmpId);

            var projectInfo = await (from emp in _context.EmployeeInformations
                                     join proj in _context.ProjectInformations
                                         on emp.ProjectId equals proj.ProjectId
                                     where emp.EmpId == ticket.EmpId
                                     select new
                                     {
                                         DmEmail = proj.DmemailId,
                                         proj.ProjectName,
                                         departmentId = proj.DepartmentID,
                                         proj.Dm
                                     }).FirstOrDefaultAsync();

            if (employee == null || projectInfo == null)
            {
                return (false, "Employee or Project information missing.");
            }

            var subject = $" {ticket.RequestType} Ticket Status Details - {ticket.EmpId} - {employee.EmpName} - ";

            string finalBody;
            if (ticket.RequestType is "ReportingChange" or "ManagerChange" or "DepartmentChange")
            {
                finalBody = EmailContentforRDRequestChange.EmailContentForRDC
                    .Replace("{EMP_ID}", ticket.EmpId)
                    .Replace("{EMP_NAME}", employee.EmpName)
                    .Replace("{PROJECT_CODE}", employee.ProjectId)
                    .Replace("{PROJECT_Name}", projectInfo.ProjectName)
                    .Replace("{DEPARTMENT_ID}", projectInfo.departmentId)
                    .Replace("{ReportingManager}", projectInfo.Dm)
                    .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                    .Replace("{Request_Status}", "Approved")
                    .Replace("{user_name}", approvedBy)
                    .Replace("{ACTION_LINK}", actionLink);

            }
            else if (ticket.RequestType is "Deallocation")
            {
                finalBody = EmailContentforDeallocation.EmailContentForDM
                    .Replace("{EMP_ID}", ticket.EmpId)
                    .Replace("{EMP_NAME}", employee.EmpName)
                    .Replace("{PROJECT_CODE}", employee.ProjectId)
                    .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                    .Replace("{Request_Status}", "Approved")
                    .Replace("{user_name}", approvedBy)
                    .Replace("{ACTION_LINK}", actionLink);
            }
            else
            {
                finalBody = EmailContentforAllocation.EmailContentForAM
                .Replace("{EMP_ID}", ticket.EmpId)
                .Replace("{EMP_NAME}", employee.EmpName)
                .Replace("{PROJECT_CODE}", employee.ProjectId)
                .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                .Replace("{Request_Status}", "Approved")
                .Replace("{user_name}", approvedBy)
                .Replace("{ACTION_LINK}", actionLink);
            }

            try
            {
                await _emailService.SendEmailAsync(
                    toEmails: gdoEmailIds,
                    subject: subject,
                    body: finalBody,
                    isHtml: true,
                    ccEmails: new List<string> { approvedBy, ticket.RequestedBy! });

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Approval of the ticket failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteRequestAsync(int ticketId, string requestedBy)
        {
            var ticket = await _context.TicketingTables.FindAsync(ticketId);

            if (ticket == null || ticket.Status != "Open" || ticket.RequestedBy != requestedBy)
                return (false, "Invalid request for deletion.");

            var employee = await _context.EmployeeInformations
                .FirstOrDefaultAsync(e => e.EmpId == ticket.EmpId);

            if (employee == null)
                return (false, "Employee not found.");
            
            _context.TicketingTables.Remove(ticket);
            employee.Deallocation = false;
            _context.EmployeeInformations.Update(employee);
            ticket.StartDate = DateTime.Now;

            var gdoEmailIds = await _context.Logins
                .Where(login => login.Role == "GDO")
                .Select(login => login.EmailId)
                .ToListAsync();

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

            if (projectInfo == null)
                return (false, "Project info not found.");

            string subject = $"{ticket.RequestType} Ticket Status Details - {ticket.EmpId} - {employee.EmpName} - ";

            string finalBody;
            if (ticket.RequestType is "ReportingChange" or "ManagerChange" or "DepartmentChange")
            {
                finalBody = EmailContentforRDRequestChange.EmailContentForRDC
                    .Replace("{EMP_ID}", ticket.EmpId)
                    .Replace("{EMP_NAME}", employee.EmpName)
                    .Replace("{PROJECT_CODE}", employee.ProjectId)
                    .Replace("{PROJECT_Name}", projectInfo.ProjectName)
                    .Replace("{DEPARTMENT_ID}", projectInfo.departmentId)
                    .Replace("{ReportingManager}", projectInfo.Dm)
                    .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                    .Replace("{Request_Status}", "Deleted")
                    .Replace("{user_name}", requestedBy)
                    .Replace("{ACTION_LINK}", actionLink);
            }
            else if(ticket.RequestType is "Deallocation")
            {
                finalBody = EmailContentforDeallocation.EmailContentForDM
                    .Replace("{EMP_ID}", ticket.EmpId)
                    .Replace("{EMP_NAME}", employee.EmpName)
                    .Replace("{PROJECT_CODE}", employee.ProjectId)
                    .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                    .Replace("{Request_Status}", "Deleted")
                    .Replace("{user_name}", requestedBy)
                    .Replace("{ACTION_LINK}", actionLink);
            }
            else
            {
                finalBody = EmailContentforAllocation.EmailContentForAM
                .Replace("{EMP_ID}", ticket.EmpId)
                .Replace("{EMP_NAME}", employee.EmpName)
                .Replace("{PROJECT_CODE}", employee.ProjectId)
                .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                .Replace("{Request_Status}", "Deleted")
                .Replace("{user_name}", requestedBy)
                .Replace("{ACTION_LINK}", actionLink);
            }

            try
            {
                await _emailService.SendEmailAsync(
                    toEmails: gdoEmailIds,
                    subject: subject,
                    body: finalBody,
                    isHtml: true,
                    ccEmails: new List<string> { requestedBy, ticket.RequestedBy });

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Email sending failed: {ex.Message}");
            }
        }
        public async Task<(bool Success, string? ErrorMessage)> RejectRequestAsync(int ticketId, string userName, List<string> userRoles)
        {
            var ticket = await _context.TicketingTables.FindAsync(ticketId);

            if (ticket == null || !(ticket.Status == "InProgress" || ticket.Status == "Open") ||
                !(userRoles.Contains("GDO") || userRoles.Contains("DM")))
            {
                return (false, "Invalid request for rejection.");
            }

            ticket.Status = "Rejected";
            ticket.ApprovedBy = userName;
            ticket.ApprovedDate = DateTime.Now;
            ticket.StartDate = DateTime.Now;

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
                                         proj.Dm
                                     }).FirstOrDefaultAsync();

            if (projectInfo == null || employee == null)
            {
                return (false, "Employee or project info not found.");
            }

            string subject = $" {ticket.RequestType} Ticket Status Details - {ticket.EmpId} - {employee.EmpName} - ";
            string finalBody;

            if (ticket.RequestType is "ReportingChange" or "ManagerChange" or "DepartmentChange")
            {
                finalBody = EmailContentforRDRequestChange.EmailContentForRDC
                    .Replace("{EMP_ID}", ticket.EmpId)
                    .Replace("{EMP_NAME}", employee.EmpName)
                    .Replace("{PROJECT_CODE}", employee.ProjectId)
                    .Replace("{PROJECT_Name}", projectInfo.ProjectName)
                    .Replace("{DEPARTMENT_ID}", projectInfo.departmentId)
                    .Replace("{ReportingManager}", projectInfo.Dm)
                    .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                    .Replace("{Request_Status}", "Rejected")
                    .Replace("{user_name}", userName)
                    .Replace("{ACTION_LINK}", actionLink);
            }
            else if(ticket.RequestType is "Deallocation")
            {
                finalBody = EmailContentforDeallocation.EmailContentForDM
                    .Replace("{EMP_ID}", ticket.EmpId)
                    .Replace("{EMP_NAME}", employee.EmpName)
                    .Replace("{PROJECT_CODE}", employee.ProjectId)
                    .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                    .Replace("{Request_Status}", "Rejected")
                    .Replace("{user_name}", userName)
                    .Replace("{ACTION_LINK}", actionLink);
            }
            else
            {
                finalBody = EmailContentforAllocation.EmailContentForAM
                .Replace("{EMP_ID}", ticket.EmpId)
                .Replace("{EMP_NAME}", employee.EmpName)
                .Replace("{PROJECT_CODE}", employee.ProjectId)
                .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                .Replace("{Request_Status}", "Deleted")
                .Replace("{user_name}", userName)
                .Replace("{ACTION_LINK}", actionLink);
            }

            try
            {
                await _emailService.SendEmailAsync(
                    toEmails: gdoEmailIds,
                    subject: subject,
                    body: finalBody,
                    isHtml: true,
                    ccEmails: new List<string> { userName, ticket.RequestedBy! });

                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Rejection email failed: {ex.Message}");
            }
        }


    }
}