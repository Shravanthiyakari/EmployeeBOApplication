using EmployeeBOApp.BussinessLayer.Interfaces;
using EmployeeBOApp.EmailsContent;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;

namespace EmployeeBOApp.Business.Implementations
{
    public class AllocationService : IAllocationService
    {
        private readonly IAllocationRepository _repository;
        private readonly IEmailService _emailService;

        public AllocationService(IAllocationRepository repository, IEmailService emailService)
        {
            _repository = repository;
            _emailService = emailService;
        }

        public List<string> GetShortProjectNames(string currentUserEmail)
        {
            return _repository.GetShortProjectNames(currentUserEmail);
        }

        public object GetProjectDetailsByShortName(string shortProjectName)
        {
            var project = _repository.GetProjectByShortName(shortProjectName);

            if (project == null)
            {
                return new { success = false, message = "Project not found" };
            }

            return new
            {
                success = true,
                projectCode = project.ProjectId,
                projectName = project.ProjectName,
                projectManager = project.Pm,
                deliveryManager = project.Dm,
                pmEmail = project.PmemailId,
                dmEmail = project.DmemailId
            };
        }

        public async Task<(bool Success, string Message)> SubmitAllocationRequest(TicketingTable ticket, string currentUserEmail)
        {
            var existingRequest = await _repository.GetExistingOpenAllocationRequest(ticket.EmpId!);

            if (existingRequest != null)
            {
                return (false, "An allocation request is already in progress for this employee.");
            }

            var employee = _repository.GetEmployeeById(ticket.EmpId!);

            if (employee != null && !employee.Deallocation)
            {
                return (false, "This employee is already allocated.");
            }

            if (employee == null)
            {
                employee = new EmployeeInformation
                {
                    EmpId = ticket.EmpId!,
                    EmpName = ticket.EmpName!,
                    ProjectId = ticket.ProjectId,
                    Deallocation = false
                };
                _repository.AddEmployee(employee);
            }

            if (employee.Deallocation)
            {
                return (false, "Deallocation process not completed for this employee.");
            }

            var projectInfo = await _repository.GetProjectInfoByEmpId(ticket.EmpId!);

            if (projectInfo == null)
            {
                return (false, "Project information not found for this employee.");
            }

            ticket.Status = "Open";
            ticket.RequestedDate = DateTime.Now;
            ticket.RequestType = "Allocation";

            string actionLink = "<a class='action-link' href='https://localhost:7168/Login/Login'>Click here to take decision</a>";
            string subject = $"Assignment Request - {employee.EmpName} - {ticket.EmpId}";

            string finalBody = EmailContentforAllocation.EmailContentForAM
                .Replace("{EMP_ID}", ticket.EmpId)
                .Replace("{EMP_NAME}", employee.EmpName)
                .Replace("{PROJECT_CODE}", employee.ProjectId)
                .Replace("{START_DATE}", ticket.StartDate?.ToString("dd-MM-yyyy"))
                .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
                .Replace("{Request_Status}", "Submitted")
                .Replace("{user_name}", currentUserEmail)
                .Replace("{ACTION_LINK}", actionLink);

            try
            {
                await _emailService.SendEmailAsync(
                    new List<string> { projectInfo.DmemailId },
                    subject,
                    finalBody,
                    true,
                    new List<string> { ticket.RequestedBy! });

                await _repository.AddTicketAsync(ticket);
            }
            catch (Exception ex)
            {
                return (false, $"Allocation saved, but email failed: {ex.Message}");
            }

            return (true, "Allocation submitted and email sent successfully!");
        }
    }
}