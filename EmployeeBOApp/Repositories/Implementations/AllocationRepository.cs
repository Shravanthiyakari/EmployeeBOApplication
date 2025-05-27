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

        public AllocationRepository(EmployeeDatabaseContext context)
        {
            _context = context;
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

        public ProjectInformation? GetProjectByShortName(string shortProjectName)
        {
            return _context.ProjectInformations
                .FirstOrDefault(p => p.ShortProjectName == shortProjectName);
        }

        public async Task<TicketingTable?> GetExistingOpenAllocationRequest(string empId)
        {
            return await _context.TicketingTables
                .FirstOrDefaultAsync(t =>
                    t.EmpId == empId &&
                    t.RequestType == "Allocation" &&
                    t.Status != "Closed");
        }

        public async Task<TicketingTable?> GetExistingBGVRequest(string empId)
        {
            return await _context.TicketingTables
                .FirstOrDefaultAsync(t =>
                    t.EmpId == empId &&
                    t.RequestType == "BGV");
        }

        public EmployeeInformation? GetEmployeeById(string empId)
        {
            return _context.EmployeeInformations.FirstOrDefault(e => e.EmpId == empId);
        }

        public void AddEmployee(EmployeeInformation employee)
        {
            _context.EmployeeInformations.Add(employee);
            _context.SaveChanges();
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


        public async Task<ProjectInformation?> GetProjectInfoByEmpId(string empId)
        {
            return await (from emp in _context.EmployeeInformations
                          join proj in _context.ProjectInformations
                              on emp.ProjectId equals proj.ProjectId
                          where emp.EmpId == empId
                          select proj).FirstOrDefaultAsync();
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
    }

}