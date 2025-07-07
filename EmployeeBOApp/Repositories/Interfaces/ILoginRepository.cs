using EmployeeBOApp.Models;

namespace EmployeeBOApp.Repositories.Interfaces
{
    public interface ILoginRepository
    {
        Task<Login> GetLoginByEmailAsync(string emailId);
    }
}
