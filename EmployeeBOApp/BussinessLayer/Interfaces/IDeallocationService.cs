using EmployeeBOApp.Models;

namespace EmployeeBOApp.BussinessLayer.Interfaces
{
    public interface IDeallocationService
    {
        Task<List<string>> GetShortProjectNamesForUserAsync(string userEmail);
        Task<List<object>> GetEmployeesByProjectAsync(string shortProjectName);
        Task<(bool Success, string? Message)> SubmitDeallocationAsync(TicketingTable ticket);
    }
}
