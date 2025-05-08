using EmployeeBOApp.Models;
namespace EmployeeBOApp.Repositories.Interfaces
{
    public interface IViewRepository
    {

        Task<(List<TicketingTable> Tickets, int TotalCount)> GetFilteredTicketsAsync(string? searchQuery,string? requestType, string? userEmail,List<string> userRoles,int page,int pageSize = 10);
        Task<List<TicketingTable>> GetTicketsForExportAsync(string? searchQuery,string? requestType,string? userEmail,List<string> userRoles);
        Task<(bool Success, string? ErrorMessage)> CloseRequestAsync(int ticketId, string approvedBy, List<string> userRoles);
        Task<(bool Success, string? ErrorMessage)> ApproveRequestAsync(int ticketId, string approvedBy, List<string> userRoles);
        Task<(bool Success, string? ErrorMessage)> DeleteRequestAsync(int ticketId, string requestedBy);
        Task<(bool Success, string? ErrorMessage)> RejectRequestAsync(int ticketId, string userName, List<string> userRoles);

    }
}