//using EmployeeBOApp.Models;
//using System.Threading.Tasks;

//namespace EmployeeBOApp.Repositories.Interfaces
//{
//    public interface IAllocationRepository
//    {
//        List<string> GetShortProjectNames(string currentUserEmail);
//        object GetProjectDetailsByShortName(string shortProjectName);
//        Task<(bool Success, string Message)> SubmitAllocationRequest(TicketingTable ticket, string currentUserEmail);
//    }
//}
using EmployeeBOApp.Models;
using System.Threading.Tasks;

namespace EmployeeBOApp.Repositories.Interfaces
{
    public interface IAllocationRepository
    {
        List<string> GetShortProjectNames(string currentUserEmail);
        ProjectInformation? GetProjectByShortName(string shortProjectName);
        Task<TicketingTable?> GetExistingOpenAllocationRequest(string empId);
        Task<TicketingTable?> GetExistingBGVRequest(string empId);
        EmployeeInformation? GetEmployeeById(string empId);
        Task<ProjectInformation?> GetProjectInfoByEmpId(string empId);
        Task AddTicketAsync(TicketingTable ticket);
        Task SaveChangesAsync();
        void AddEmployee(EmployeeInformation employee);
        void UpdateEmployee(EmployeeInformation employee);
    }
}
