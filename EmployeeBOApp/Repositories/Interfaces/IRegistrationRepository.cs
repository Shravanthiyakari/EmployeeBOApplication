using EmployeeBOApp.Models;

namespace EmployeeBOApp.Repositories.Interfaces
{
    public interface IRegistrationRepository
    {
        Task<bool> AddLoginAsync(Login model);
    }
}
