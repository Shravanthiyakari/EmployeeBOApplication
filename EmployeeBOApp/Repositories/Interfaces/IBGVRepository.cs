using EmployeeBOApp.Models;

namespace EmployeeBOApp.Repositories.Interfaces
{
    public interface IBGVRepository
    {
        Task<object> CheckExistingBGV(string empId);
        Task<(bool Success, string Message)> InitiateBGV(EmployeeInformation model, bool confirm, string requestedBy);
        Task<List<string>> GetHRCCEmails();
        Task<string> GetPMNameByEmail(string email);
    }
}
