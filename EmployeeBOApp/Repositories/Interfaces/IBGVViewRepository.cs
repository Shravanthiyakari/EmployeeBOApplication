using EmployeeBOApp.Models;

namespace EmployeeBOApp.Repositories.Interfaces
{
    public interface IBGVViewRepository
    {
        Task<(List<TicketingTable>, int currentPage, int totalPages)> GetPaginatedTickets(string searchQuery, string requestType, int page, int pageSize);
        Task<(bool success, string message, TicketingTable ticket, string pm, string dm, string latestBgvId)> SubmitTicket(int id, string bgvId, string empId, string approvedBy);
        List<TicketingTable> GetFilteredTickets(string searchQuery);
    }
}
