using EmployeeBOApp.Data;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeBOApp.Repositories.Implementations
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly EmployeeDatabaseContext _context;

        public RegistrationRepository(EmployeeDatabaseContext context)
        {
            _context = context;
        }

        public async Task<bool> AddLoginAsync(Login model)
        {
            try
            {
                await _context.Logins.AddAsync(model);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                // Optionally log the error
                return false;
            }
        }
    }
}
