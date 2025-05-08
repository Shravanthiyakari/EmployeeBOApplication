using EmployeeBOApp.Models;
using System.Threading.Tasks;

namespace EmployeeBOApp.Repositories.Interfaces
{
    public interface IAllocationRepository
    {
        List<string> GetShortProjectNames(string currentUserEmail);
        object GetProjectDetailsByShortName(string shortProjectName);
        Task<(bool Success, string Message)> SubmitAllocationRequest(TicketingTable ticket, string currentUserEmail);
    }
}

