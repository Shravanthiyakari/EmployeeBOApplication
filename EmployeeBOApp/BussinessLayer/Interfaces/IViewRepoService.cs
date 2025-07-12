using EmployeeBOApp.Models;

namespace EmployeeBOApp.BussinessLayer.Interfaces
{
    public interface IViewRepoService
    {
        Task<(List<TicketingTable> Tickets, int TotalCount)> GetFilteredTicketsAsync(string? searchQuery, string? requestType, string? userEmail, List<string> userRoles, int page);
        Task<List<TicketingTable>> GetTicketsForExportAsync(string? searchQuery, string? requestType, string? userEmail, List<string> userRoles);
        Task<(bool Success, string? ErrorMessage)> CloseRequestAsync(int ticketId, string approvedBy, List<string> userRoles);
        Task<(bool Success, string? ErrorMessage)> ApproveRequestAsync(int ticketId, string approvedBy, List<string> userRoles);
        Task<(bool Success, string? ErrorMessage)> DeleteRequestAsync(int ticketId, string requestedBy);
        Task<(bool Success, string? ErrorMessage)> RejectRequestAsync(int ticketId, string userName, List<string> userRoles);
    }
}
