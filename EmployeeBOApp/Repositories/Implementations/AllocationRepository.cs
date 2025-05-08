using EmployeeBOApp.Data;
using EmployeeBOApp.EmailsContent;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace EmployeeBOApp.Repositories.Implementations
{
    public class AllocationRepository : IAllocationRepository
    {
        private readonly EmployeeDatabaseContext _context;
        private readonly IEmailService _emailService;
        private string actionLink = "";
        public AllocationRepository(EmployeeDatabaseContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        public List<string> GetShortProjectNames(string currentUserEmail)
        {
            return _context.ProjectInformations
                .Where(p =>
                    (p.PmemailId == currentUserEmail || p.DmemailId == currentUserEmail) &&
                    p.ShortProjectName != null)
                .Select(p => p.ShortProjectName!)
                .Distinct()
                .ToList();
        }

        public object GetProjectDetailsByShortName(string shortProjectName)
        {
            var project = _context.ProjectInformations
                .FirstOrDefault(p => p.ShortProjectName == shortProjectName);

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
            var existingRequest = await _context.TicketingTables
                .Where(t => t.EmpId == ticket.EmpId &&
                            t.RequestType == "Allocation" &&
                            t.Status != "Closed")
                .FirstOrDefaultAsync();

            if (existingRequest != null)
                return (false, "An allocation request is already in progress for this employee. Please wait until it's closed before submitting a new one.");

            var employee = _context.EmployeeInformations
                .FirstOrDefault(e => e.EmpId == ticket.EmpId);

            if (employee != null && !employee.Deallocation)
                return (false, "This employee is already allocated. You cannot submit the request again.");

            if (employee == null)
            {
                employee = new EmployeeInformation
                {
                    EmpId = ticket.EmpId!,
                    EmpName = ticket.EmpName!,
                    ProjectId = ticket.ProjectId,
                    Deallocation = false
                };
                _context.EmployeeInformations.Add(employee);
                _context.SaveChanges();
            }

            if (employee.Deallocation)
                return (false, "Deallocation process is not yet completed for this employee.");

            var projectInfo = await (from emp in _context.EmployeeInformations
                                     join proj in _context.ProjectInformations
                                         on emp.ProjectId equals proj.ProjectId
                                     where emp.EmpId == ticket.EmpId
                                     select new
                                     {
                                         DmEmail = proj.DmemailId,
                                         proj.ProjectName
                                     }).FirstOrDefaultAsync();

            ticket.Status = "Open";
            ticket.RequestedDate = DateTime.Now;
            ticket.RequestType = "Allocation";

            if (ticket.Status == "Open")
            {
                actionLink = "<a class='action-link' href='https://localhost:7168/Login/Login'>Click here to take decision</a>";
            }

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
                    toEmails: new List<string> { projectInfo!.DmEmail },
                    subject: subject,
                    body: finalBody,
                    isHtml: true,
                    ccEmails: new List<string> { ticket.RequestedBy! }
                );

                
                _context.TicketingTables.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return (false, $"Allocation saved, but email failed: {ex.Message}");
            }

            return (true, "Allocation submitted and email sent successfully!");
        }

    }
}
