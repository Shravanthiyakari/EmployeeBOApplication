using EmployeeBOApp.BussinessLayer.Interfaces;
using EmployeeBOApp.Data;
using EmployeeBOApp.EmailsContent;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;

namespace EmployeeBOApp.BussinessLayer.Implementations
{
    public class DeallocationService : IDeallocationService
    {
        private readonly IDeallocationRepository _repository;
        private readonly IEmailService _emailService;
        private string actionLink = "";

        public DeallocationService(IDeallocationRepository repository, IEmailService emailService)
        {
            _repository = repository;
            _emailService = emailService;
        }

        public async Task<List<string>> GetShortProjectNamesForUserAsync(string userEmail)
        {
            return await _repository.GetShortProjectNamesForUserAsync(userEmail);
        }
        public async Task<List<object>> GetEmployeesByProjectAsync(string shortProjectName)
        {
            return await _repository.GetEmployeesByProjectAsync(shortProjectName);
        }
        public async Task<(bool Success, string? Message)> SubmitDeallocationAsync(TicketingTable ticket)
        {
            var employee = await _repository.GetEmployeeByIdAsync(ticket.EmpId!);
            if (employee == null)
                return (false, "Employee not found.");

            if (employee.Deallocation == true)
                return (false, "Deallocation is already submitted.");

            var existingRequest = await _repository.GetOpenAllocationRequestAsync(ticket.EmpId!);
            if (existingRequest != null)
                return (false, "An allocation request is already in progress for this employee. Please wait until it's closed.");

            var projectInfo = await _repository.GetProjectInfoByEmpIdAsync(ticket.EmpId!);
            if (projectInfo == null)
                return (false, "Project information not found.");

            // Set ticket details
            ticket.Status = "Open";
            ticket.RequestedDate = DateTime.Now;
            ticket.RequestType = "Deallocation";
            if (ticket.Status == "Open")
            {
                actionLink = "<a class='action-link' href='https://localhost:7168/Login/Login'>Click here to take decision</a>";
            }
            // Update employee Deallocation status
            employee.Deallocation = true;

            // Add ticket to the context (we track changes)
            await _repository.AddTicketAsync(ticket);

            _repository.UpdateEmployee(employee);
            // Update the employee (we track changes)

            // Prepare email content
            string dmEmail = projectInfo.Value.DmEmail!;
            string projectName = projectInfo.Value.ProjectName!;
            string subject = $"Deallocation Request - {projectName} - {ticket.EmpId} - {employee.EmpName}";

            string finalBody = EmailContentforDeallocation.EmailContentForDM
                .Replace("{EMP_ID}", ticket.EmpId)
                .Replace("{EMP_NAME}", employee.EmpName)
                .Replace("{PROJECT_CODE}", employee.ProjectId)
                .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                .Replace("{Request_Status}", "Submitted")
                .Replace("{user_name}", ticket.RequestedBy)
                 .Replace("{ACTION_LINK}", actionLink);

            try
            {
                await _emailService.SendEmailAsync(
                    toEmails: new List<string> { dmEmail },
                    subject: subject,
                    body: finalBody,
                    isHtml: true,
                    ccEmails: new List<string> { ticket.RequestedBy! });

                // Save all changes in one go
                await _repository.SaveChangesAsync();

                return (true, "Deallocation submitted and email sent successfully!");
            }
            catch (Exception ex)
            {
                return (false, $"Deallocation saved, but email failed: {ex.Message}");
            }
        }  
    }
}
