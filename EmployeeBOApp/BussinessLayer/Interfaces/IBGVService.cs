using EmployeeBOApp.Models;

namespace EmployeeBOApp.BusinessLayer.Interfaces
{
    public interface IBGVService
    {
        Task<object> CheckExistingBGVAsync(string empId);
        Task<(bool Success, string Message)> InitiateBGVAsync(EmployeeInformation model, bool confirm, string userName);
    }
}
