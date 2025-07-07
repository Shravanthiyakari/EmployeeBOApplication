using EmployeeBOApp.Data;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace EmployeeBOApp.Repositories.Implementations
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeDatabaseContext _context;

        public EmployeeRepository(EmployeeDatabaseContext context)
        {
            _context = context;
        }

        public List<ProjectInformation> GetProjectsByUser(string email, string role)
        {
            return _context.ProjectInformations
                .Where(p => (role == "PM" && p.PmemailId == email) ||
                            (role == "DM" && p.DmemailId == email))
                .ToList();
        }

        public List<EmployeeInformation> GetEmployees(string selectedProjectId)
        {
            IQueryable<EmployeeInformation> query = _context.EmployeeInformations
                .Include(e => e.BgvMap)
                .Include(e => e.Project);

            if (selectedProjectId != "Select")
            {
                query = query.Where(e => e.ProjectId == selectedProjectId);
            }

            return query.ToList();
        }

    }
}
