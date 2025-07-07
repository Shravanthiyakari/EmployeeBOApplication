using EmployeeBOApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeBOApp.Repositories.Interfaces
{
    public interface IReportingRepository
    {
        List<string> GetShortProjectNames(string userEmail);
        ProjectInformation GetProjectDetails(string shortProjectName);
        List<EmployeeInformation> GetEmployeesByProject(string shortProjectName);
        Task<EmployeeInformation> GetEmployeeByIdAsync(string empId);
        Task<(string DmEmail, string ProjectName, string DepartmentId, string Dm)> GetProjectInfoByEmployeeIdAsync(string empId);
        Task SaveTicketAsync(TicketingTable ticket, EmployeeInformation employee);
        Task<TicketingTable> GetConflictingExclusiveRequestAsync(string empId, string requestType, IEnumerable<string> exclusiveTypes);
        Task<TicketingTable> GetDuplicateRequestAsync(string empId, string requestType);

        //Task<(bool Success, string Message)> SendReportingChangeEmailAsync(TicketingTable ticket, EmployeeInformation employee, (string DmEmail, string ProjectName, string DepartmentId, string Dm) projectInfo, string requestedByEmail);
    }


}
