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
          int page)
        {
            const int pageSize = 10;

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

        public async Task<TicketingTable?> GetTicketByIdAsync(int ticketId)
        {
            return await _context.TicketingTables.FindAsync(ticketId);
        }

        public async Task<List<string>> GetGdoEmailIdsAsync()
        {
            return await _context.Logins
                .Where(login => login.Role == "GDO")
                .Select(login => login.EmailId)
                .ToListAsync();
        }

        public async Task<EmployeeInformation?> GetEmployeeByEmpIdAsync(string empId)
        {
            return await _context.EmployeeInformations
                .FirstOrDefaultAsync(e => e.EmpId == empId);
        }

        public void DeleteTicket(TicketingTable ticket)
        {
            _context.TicketingTables.Remove(ticket);
        }

        public void UpdateTicket(EmployeeInformation employee)
        {
            _context.EmployeeInformations.Update(employee);
        }

        public async Task<(string DmEmail, string ProjectName, string DepartmentId, string Dm)> GetProjectInfoByEmpIdAsync(string empId)
        {
            var result = await (from emp in _context.EmployeeInformations
                                join proj in _context.ProjectInformations
                                    on emp.ProjectId equals proj.ProjectId
                                where emp.EmpId == empId
                                select new
                                {
                                    DmEmail = proj.DmemailId,
                                    proj.ProjectName,
                                    DepartmentId = proj.DepartmentID.ToString(), // Cast to string here!
                                    proj.Dm
                                }).FirstOrDefaultAsync();

            if (result == null)
            {
                return (null!, null!, null!, null!);
            }

            return (result.DmEmail, result.ProjectName, result.DepartmentId, result.Dm);
        }


        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}