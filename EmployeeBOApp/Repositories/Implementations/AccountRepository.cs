using EmployeeBOApp.Repositories.Interfaces;

namespace EmployeeBOApp.Repositories.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        public void SubmitDetails()
        {
            // Save data to DB (dummy implementation for now)
            // e.g., _context.Accounts.Add(account); _context.SaveChanges();
        }
    }
}
