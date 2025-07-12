using EmployeeBOApp.Models;
using System.Net.Sockets;
namespace EmployeeBOApp.Repositories.Interfaces
{
    public interface IViewRepository
    {

        Task<(List<TicketingTable> Tickets, int TotalCount)> GetFilteredTicketsAsync(string? searchQuery,string? requestType, string? userEmail,List<string> userRoles,int page);
        Task<List<TicketingTable>> GetTicketsForExportAsync(string? searchQuery,string? requestType,string? userEmail,List<string> userRoles);
        Task<(string DmEmail, string ProjectName, string DepartmentId, string Dm)> GetProjectInfoByEmpIdAsync(string empId);
        Task SaveChangesAsync();
        Task<TicketingTable?> GetTicketByIdAsync(int ticketId);
        Task<List<string>> GetGdoEmailIdsAsync();
        Task<EmployeeInformation?> GetEmployeeByEmpIdAsync(string empId);
        void DeleteTicket(TicketingTable ticket);
        void UpdateTicket(EmployeeInformation employee);
    }
}