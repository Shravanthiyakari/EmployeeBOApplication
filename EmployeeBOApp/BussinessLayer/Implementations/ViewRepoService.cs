using EmployeeBOApp.BussinessLayer.Interfaces;
using EmployeeBOApp.Data;
using EmployeeBOApp.EmailsContent;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;

namespace EmployeeBOApp.BussinessLayer.Implementations
{
    public class ViewRepoService : IViewRepoService
    {
        private readonly IViewRepository _repository;
        private readonly IEmailService _emailService;
        private string actionLink = "";
        private readonly EmployeeDatabaseContext _context;

        public ViewRepoService(IViewRepository repository, IEmailService emailService)
        {
            _repository = repository;
            _emailService = emailService;
        }

        public async Task<(List<TicketingTable> Tickets, int TotalCount)> GetFilteredTicketsAsync(
          string? searchQuery,
          string? requestType,
          string? userEmail,
          List<string> userRoles,
          int page)
        {
            return await _repository.GetFilteredTicketsAsync(
                            searchQuery, requestType, userEmail, userRoles, page);
        }
        public async Task<List<TicketingTable>> GetTicketsForExportAsync(
       string? searchQuery,
       string? requestType,
       string? userEmail,
       List<string> userRoles)
        {
            return await _repository.GetTicketsForExportAsync(
                            searchQuery, requestType, userEmail, userRoles);
        }
        public async Task<(bool Success, string? ErrorMessage)> CloseRequestAsync(int ticketId, string approvedBy, List<string> userRoles)
        {
            var ticket = await _repository.GetTicketByIdAsync(ticketId);

            if (ticket == null || ticket.Status != "InProgress" || !userRoles.Contains("GDO"))
            {
                return (false, "Invalid request for approval.");
            }

            ticket.Status = "Closed";
            ticket.ApprovedDate = DateTime.Now;
            ticket.ApprovedBy = approvedBy;
            ticket.StartDate = DateTime.Now;

            string actionLink = string.Empty;

            if (ticket.Status != "Closed")
            {
                actionLink = "<a class='action-link' href='https://localhost:7168/Login/Login'>Click here to take decision</a>";
            }

            var gdoEmailIds = await _repository.GetGdoEmailIdsAsync();

            var employee = await _repository.GetEmployeeByEmpIdAsync(ticket.EmpId);

            var projectInfo = await _repository.GetProjectInfoByEmpIdAsync(ticket.EmpId);

            if (employee == null || string.IsNullOrEmpty(projectInfo.ProjectName))
            {
                return (false, "Employee or Project info not found.");
            }

            var subject = $"{ticket.RequestType} Ticket Status Details - {ticket.EmpId} - {employee.EmpName} - ";

            string finalBody;

            if (ticket.RequestType is "ReportingChange" or "ManagerChange" or "DepartmentChange")
            {
                finalBody = EmailContentforRDRequestChange.EmailContentForRDC
                    .Replace("{EMP_ID}", ticket.EmpId)
                    .Replace("{EMP_NAME}", employee.EmpName)
                    .Replace("{PROJECT_CODE}", employee.ProjectId)
                    .Replace("{PROJECT_Name}", projectInfo.ProjectName)
                    .Replace("{DEPARTMENT_ID}", projectInfo.DepartmentId.ToString())
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

                await _repository.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Approval of the ticket failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ApproveRequestAsync(int ticketId, string approvedBy, List<string> userRoles)
        {
            var ticket = await _repository.GetTicketByIdAsync(ticketId);

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

            var gdoEmailIds = await _repository.GetGdoEmailIdsAsync();

            var employee = await _repository.GetEmployeeByEmpIdAsync(ticket.EmpId);

            var projectInfo = await _repository.GetProjectInfoByEmpIdAsync(ticket.EmpId);

            if ((string.IsNullOrEmpty(projectInfo.ProjectName) &&
                     string.IsNullOrEmpty(projectInfo.DmEmail) &&
                     string.IsNullOrEmpty(projectInfo.DepartmentId) &&
                     string.IsNullOrEmpty(projectInfo.Dm)) || employee == null)
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
                    .Replace("{DEPARTMENT_ID}", projectInfo.DepartmentId)
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

                await _repository.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Approval of the ticket failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteRequestAsync(int ticketId, string requestedBy)
        {
            var ticket = await _repository.GetTicketByIdAsync(ticketId);

            if (ticket == null || ticket.Status != "Open" || ticket.RequestedBy != requestedBy)
                return (false, "Invalid request for deletion.");
        
            var employee = await _repository.GetEmployeeByEmpIdAsync(ticket.EmpId);

            if (employee == null)
                return (false, "Employee not found.");

            _repository.DeleteTicket(ticket);
            employee.Deallocation = false;

            _repository.UpdateTicket(employee);

            ticket.StartDate = DateTime.Now;

            var gdoEmailIds = await _repository.GetGdoEmailIdsAsync();

            var projectInfo = await _repository.GetProjectInfoByEmpIdAsync(ticket.EmpId);

            if ((string.IsNullOrEmpty(projectInfo.ProjectName) &&
                     string.IsNullOrEmpty(projectInfo.DmEmail) &&
                     string.IsNullOrEmpty(projectInfo.DepartmentId) &&
                     string.IsNullOrEmpty(projectInfo.Dm)))  
            {
                return (false, "Project info not found.");
            }
            string subject = $"{ticket.RequestType} Ticket Status Details - {ticket.EmpId} - {employee.EmpName} - ";

            string finalBody;
            if (ticket.RequestType is "ReportingChange" or "ManagerChange" or "DepartmentChange")
            {
                finalBody = EmailContentforRDRequestChange.EmailContentForRDC
                    .Replace("{EMP_ID}", ticket.EmpId)
                    .Replace("{EMP_NAME}", employee.EmpName)
                    .Replace("{PROJECT_CODE}", employee.ProjectId)
                    .Replace("{PROJECT_Name}", projectInfo.ProjectName)
                    .Replace("{DEPARTMENT_ID}", projectInfo.DepartmentId)
                    .Replace("{ReportingManager}", projectInfo.Dm)
                    .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                    .Replace("{Request_Status}", "Deleted")
                    .Replace("{user_name}", requestedBy)
                    .Replace("{ACTION_LINK}", actionLink);
            }
            else if (ticket.RequestType is "Deallocation")
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

                await _repository.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Email sending failed: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RejectRequestAsync(int ticketId, string userName, List<string> userRoles)

        {
            var ticket = await _repository.GetTicketByIdAsync(ticketId);

            if (ticket == null || !(ticket.Status == "InProgress" || ticket.Status == "Open") ||
                !(userRoles.Contains("GDO") || userRoles.Contains("DM")))
            {
                return (false, "Invalid request for rejection.");
            }

            ticket.Status = "Rejected";
            ticket.ApprovedBy = userName;
            ticket.ApprovedDate = DateTime.Now;
            ticket.StartDate = DateTime.Now;

            var gdoEmailIds = await _repository.GetGdoEmailIdsAsync();

            var employee = await _repository.GetEmployeeByEmpIdAsync(ticket.EmpId);

            var projectInfo = await _repository.GetProjectInfoByEmpIdAsync(ticket.EmpId);
            
            if ((string.IsNullOrEmpty(projectInfo.ProjectName) &&
                 string.IsNullOrEmpty(projectInfo.DmEmail) &&
                 string.IsNullOrEmpty(projectInfo.DepartmentId) &&
                 string.IsNullOrEmpty(projectInfo.Dm))
                || employee == null)
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
                    .Replace("{DEPARTMENT_ID}", projectInfo.DepartmentId)
                    .Replace("{ReportingManager}", projectInfo.Dm)
                    .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                    .Replace("{Request_Status}", "Rejected")
                    .Replace("{user_name}", userName)
                    .Replace("{ACTION_LINK}", actionLink);
            }
            else if (ticket.RequestType is "Deallocation")
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
                .Replace("{Request_Status}", "Rejected")
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

                await _repository.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Rejection email failed: {ex.Message}");
            }
        }
    }
}
