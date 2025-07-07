using EmployeeBOApp.Models;

namespace EmployeeBOApp.BusinessLayer.Interfaces
{
    public interface ILoginService
    {
        Task<Login> ValidateUserAsync(string emailId);
    }
}
