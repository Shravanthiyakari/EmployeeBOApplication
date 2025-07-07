using global::EmployeeBOApp.Data;
using global::EmployeeBOApp.Models;
using global::EmployeeBOApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace EmployeeBOApp.Repositories.Implementations
{
    public class DeallocationRepository : IDeallocationRepository
    {
        private readonly EmployeeDatabaseContext _context;
        private readonly IEmailService _emailService;

        public DeallocationRepository(EmployeeDatabaseContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<List<string>> GetShortProjectNamesForUserAsync(string userEmail)
        {
            return await _context.ProjectInformations
                .Where(p => (p.PmemailId == userEmail || p.DmemailId == userEmail) && p.ShortProjectName != null)
                .Select(p => p.ShortProjectName!)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<object>> GetEmployeesByProjectAsync(string shortProjectName)
        {
            return await _context.EmployeeInformations
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
                }).Cast<object>()
                .ToListAsync();
        }

        public async Task<TicketingTable?> GetOpenAllocationRequestAsync(string empId)
        {
            return await _context.TicketingTables
                .Where(t => t.EmpId == empId && t.RequestType == "Allocation" && t.Status != "Closed")
                .FirstOrDefaultAsync();
        }

        public async Task<EmployeeInformation?> GetEmployeeByIdAsync(string empId)
        {
            return await _context.EmployeeInformations.FirstOrDefaultAsync(e => e.EmpId == empId);
        }

        public async Task<(string? DmEmail, string? ProjectName)?> GetProjectInfoByEmpIdAsync(string empId)
        {
            var result = await (from emp in _context.EmployeeInformations
                                join proj in _context.ProjectInformations
                                on emp.ProjectId equals proj.ProjectId
                                where emp.EmpId == empId
                                select new
                                {
                                    proj.DmemailId,
                                    proj.ProjectName
                                }).FirstOrDefaultAsync();

            if (result == null)
                return null;

            return (result.DmemailId, result.ProjectName); // ✅ materialize tuple after async
        }
        public void UpdateEmployee(EmployeeInformation employee)
        {
            var existingEmployee = _context.EmployeeInformations
                                           .FirstOrDefault(e => e.EmpId == employee.EmpId);

            if (existingEmployee != null)
            {
                existingEmployee.ProjectId = employee.ProjectId;
                _context.SaveChanges();
            }
        }
        public async Task AddTicketAsync(TicketingTable ticket)
        {
            _context.TicketingTables.Add(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        //public async Task<(bool Success, string? Message)> SubmitDeallocationAsync(TicketingTable ticket)
        //{
        //    var employee = await GetEmployeeByIdAsync(ticket.EmpId!);
        //    if (employee == null)
        //        return (false, "Employee not found.");

        //    if (employee.Deallocation == true)
        //        return (false, "Deallocation is already submitted.");

        //    var existingRequest = await GetOpenAllocationRequestAsync(ticket.EmpId!);
        //    if (existingRequest != null)
        //        return (false, "An allocation request is already in progress for this employee. Please wait until it's closed.");

        //    var projectInfo = await GetProjectInfoByEmpIdAsync(ticket.EmpId!);
        //    if (projectInfo == null)
        //        return (false, "Project information not found.");

        //    // Set ticket details
        //    ticket.Status = "Open";
        //    ticket.RequestedDate = DateTime.Now;
        //    ticket.RequestType = "Deallocation";
        //    if (ticket.Status == "Open")
        //    {
        //        actionLink = "<a class='action-link' href='https://localhost:7168/Login/Login'>Click here to take decision</a>";
        //    }
        //    // Update employee Deallocation status
        //    employee.Deallocation = true;

        //    // Add ticket to the context (we track changes)
        //    _context.TicketingTables.Add(ticket);

        //    // Update the employee (we track changes)
        //    _context.EmployeeInformations.Update(employee);

        //    // Prepare email content
        //    string dmEmail = projectInfo.Value.DmEmail!;
        //    string projectName = projectInfo.Value.ProjectName!;
        //    string subject = $"Deallocation Request - {projectName} - {ticket.EmpId} - {employee.EmpName}";

        //    string finalBody = EmailContentforDeallocation.EmailContentForDM
        //        .Replace("{EMP_ID}", ticket.EmpId)
        //        .Replace("{EMP_NAME}", employee.EmpName)
        //        .Replace("{PROJECT_CODE}", employee.ProjectId)
        //        .Replace("{END_DATE}", ticket.EndDate?.ToString("dd-MM-yyyy"))
        //        .Replace("{Request_Status}", "Submitted")
        //        .Replace("{user_name}", ticket.RequestedBy)
        //         .Replace("{ACTION_LINK}", actionLink);

        //    try
        //    {
        //        await _emailService.SendEmailAsync(
        //            toEmails: new List<string> { dmEmail },
        //            subject: subject,
        //            body: finalBody,
        //            isHtml: true,
        //            ccEmails: new List<string> { ticket.RequestedBy! });

        //        // Save all changes in one go
        //        await _context.SaveChangesAsync();

        //        return (true, "Deallocation submitted and email sent successfully!");
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, $"Deallocation saved, but email failed: {ex.Message}");
        //    }
        //}

    }
}



