using EmployeeBOApp;
using EmployeeBOApp.Data;
using EmployeeBOApp.EmailsContent;
using EmployeeBOApp.EmailServices;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ReportingRepository : IReportingRepository
{
    private readonly EmployeeDatabaseContext _context;
    private readonly IEmailService _emailService;
    private string actionLink = "";

    public ReportingRepository(EmployeeDatabaseContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public List<string> GetShortProjectNames(string userEmail)
    {
        return _context.ProjectInformations!
            .Where(p => (p.PmemailId == userEmail || p.DmemailId == userEmail) && p.ShortProjectName != null)
            .Select(p => p.ShortProjectName!)
            .Distinct()
            .ToList();
    }


    public ProjectInformation GetProjectDetails(string shortProjectName)
    {
        var project = _context.ProjectInformations
            .FirstOrDefault(p => p.ShortProjectName == shortProjectName);

        if (project == null)
            throw new InvalidOperationException($"Project not found for: {shortProjectName}");

        return project;
    }


    public List<EmployeeInformation> GetEmployeesByProject(string shortProjectName)
    {
        var project = _context.ProjectInformations
            .FirstOrDefault(p => p.ShortProjectName == shortProjectName);

        if (project != null)
        {
            return _context.EmployeeInformations
                .Where(e => e.ProjectId == project.ProjectId)
                .Select(e => new EmployeeInformation { EmpId = e.EmpId, EmpName = e.EmpName })
                .ToList();
        }

        return new List<EmployeeInformation>();
    }

    public async Task<EmployeeInformation> GetEmployeeByIdAsync(string empId)
    {
        var employee = await _context.EmployeeInformations.FirstOrDefaultAsync(e => e.EmpId == empId);

        if (employee == null)
            throw new InvalidOperationException($"No employee found with ID: {empId}");

        return employee;
    }



    public async Task<(string DmEmail, string ProjectName, string DepartmentId, string Dm)> GetProjectInfoByEmployeeIdAsync(string empId)
    {
        var result = await (from emp in _context.EmployeeInformations
                            join proj in _context.ProjectInformations
                                on emp.ProjectId equals proj.ProjectId
                            where emp.EmpId == empId
                            select new
                            {
                                proj.DmemailId,
                                proj.ProjectName,
                                proj.DepartmentID,
                                proj.Dm
                            }).FirstOrDefaultAsync();

        return (result!.DmemailId, result.ProjectName, result.DepartmentID, result.Dm);
    }

    public async Task SaveTicketAsync(TicketingTable ticket, EmployeeInformation employee)
    {
        _context.TicketingTables.Add(ticket);
        _context.EmployeeInformations.Update(employee);
        await _context.SaveChangesAsync();
    }
    public async Task<(bool Success, string Message)> SendReportingChangeEmailAsync(
    TicketingTable ticket,
    EmployeeInformation employee,
    (string DmEmail, string ProjectName, string DepartmentId, string Dm) projectInfo,
    string requestedByEmail)
    {
        var subject = $"{ticket.RequestType} Request - {ticket.EmpId} - {employee.EmpName}";
        if(ticket.Status=="Open")
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

