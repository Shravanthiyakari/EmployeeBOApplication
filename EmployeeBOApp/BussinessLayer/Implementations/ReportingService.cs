using EmployeeBOApp.BussinessLayer.Interfaces;
using EmployeeBOApp.EmailsContent;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;

namespace EmployeeBOApp.BussinessLayer.Implementations
{
    public class ReportingService : IReportingService
    {
        private readonly IReportingRepository _repository;
        private readonly IEmailService _emailService;
        private string actionLink = "";

        public ReportingService(IReportingRepository repository, IEmailService emailService)
        {
            _repository = repository;
            _emailService = emailService;
        }
        public  List<string> GetShortProjectNames(string userEmail)
        {
            return  _repository.GetShortProjectNames(userEmail);

        }


        public ProjectInformation GetProjectDetails(string shortProjectName)
        {
            return _repository.GetProjectDetails(shortProjectName);
        }


        public List<EmployeeInformation> GetEmployeesByProject(string shortProjectName)
        {
            return _repository.GetEmployeesByProject(shortProjectName);

        }

        public async Task<EmployeeInformation> GetEmployeeByIdAsync(string empId)
        {
            return await _repository.GetEmployeeByIdAsync(empId);

        }

        public async Task<(string DmEmail, string ProjectName, string DepartmentId, string Dm)> GetProjectInfoByEmployeeIdAsync(string empId)
        {
            return await _repository.GetProjectInfoByEmployeeIdAsync(empId);
        }

        public async Task SaveTicketAsync(TicketingTable ticket, EmployeeInformation employee)
        {
             await _repository.SaveTicketAsync(ticket, employee);

        }

        public async Task<TicketingTable> CheckForConflictingExclusiveRequestAsync(TicketingTable ticket, IEnumerable<string> exclusiveTypes)
        {
            return await _repository.GetConflictingExclusiveRequestAsync(ticket.EmpId, ticket.RequestType, exclusiveTypes);
        }

        public async Task<TicketingTable> CheckForDuplicateRequestAsync(TicketingTable ticket)
        {
            return await _repository.GetDuplicateRequestAsync(ticket.EmpId, ticket.RequestType);
        }

        public async Task<(bool Success, string Message)> SendReportingChangeEmailAsync(
        TicketingTable ticket,
        EmployeeInformation employee,
        (string DmEmail, string ProjectName, string DepartmentId, string Dm) projectInfo,
        string requestedByEmail)
        {
            var subject = $"{ticket.RequestType} Request - {ticket.EmpId} - {employee.EmpName}";
            if (ticket.Status == "Open")
            {
                actionLink = "<a class='action-link' href='https://localhost:7168/Login/Login'>Click here to take decision</a>";

            }
            string finalBody = EmailContentforRDRequestChange.EmailContentForRDC
                .Replace("{EMP_ID}", ticket.EmpId)
                .Replace("{EMP_NAME}", employee?.EmpName)
                .Replace("{PROJECT_CODE}", employee?.ProjectId)
                .Replace("{PROJECT_Name}", projectInfo.ProjectName)
                .Replace("{DEPARTMENT_ID}", projectInfo.DepartmentId)
                .Replace("{ReportingManager}", projectInfo.Dm)
                .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                .Replace("{Request_Status}", "Submitted")
                .Replace("{user_name}", requestedByEmail)
                .Replace("{ACTION_LINK}", actionLink);

            try
            {
                await _emailService.SendEmailAsync(
                    toEmails: new List<string> { projectInfo.DmEmail },
                    subject: subject,
                    body: finalBody,
                    isHtml: true,
                    ccEmails: new List<string> { requestedByEmail }
                );

                return (true, "Email sent successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Email failed: {ex.Message}");
            }
        }
    }
}
