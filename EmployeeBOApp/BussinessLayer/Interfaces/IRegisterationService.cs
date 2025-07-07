using EmployeeBOApp.Models;

namespace EmployeeBOApp.BusinessLayer.Interfaces
{
    public interface IRegistrationService
    {
        Task<bool> RegisterUserAsync(Login model);
    }
}
