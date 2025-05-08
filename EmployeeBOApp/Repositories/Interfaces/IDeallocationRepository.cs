// Interfaces/IDeallocationRepository.cs
using EmployeeBOApp.Models;
namespace EmployeeBOApp.Repositories.Interfaces
{
    
        public interface IDeallocationRepository
        {
            Task<List<string>> GetShortProjectNamesForUserAsync(string userEmail);
            Task<List<object>> GetEmployeesByProjectAsync(string shortProjectName);
            Task<TicketingTable?> GetOpenAllocationRequestAsync(string empId);
            Task<EmployeeInformation?> GetEmployeeByIdAsync(string empId);
            Task<(string? DmEmail, string? ProjectName)?> GetProjectInfoByEmpIdAsync(string empId);
        Task<(bool Success, string? Message)> SubmitDeallocationAsync(TicketingTable ticket);
    }
    }


