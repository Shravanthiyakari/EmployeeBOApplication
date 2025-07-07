using EmployeeBOApp.Data;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeBOApp.Repositories.Implementations
{
    public class LoginRepository : ILoginRepository
    {
        private readonly EmployeeDatabaseContext _context;

        public LoginRepository(EmployeeDatabaseContext context)
        {
            _context = context;
        }

        public async Task<Login> GetLoginByEmailAsync(string emailId)
        {
            return await _context.Logins
                .FirstOrDefaultAsync(u => u.EmailId == emailId);
        }
    }
}
