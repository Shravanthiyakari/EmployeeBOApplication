using EmployeeBOApp.Models;

namespace EmployeeBOApp.BussinessLayer.Interfaces
{
    public interface IAllocationService
    {
        List<string> GetShortProjectNames(string currentUserEmail);
        object GetProjectDetailsByShortName(string shortProjectName);
        Task<(bool Success, string Message)> SubmitAllocationRequest(TicketingTable ticket, string currentUserEmail);
    }
}