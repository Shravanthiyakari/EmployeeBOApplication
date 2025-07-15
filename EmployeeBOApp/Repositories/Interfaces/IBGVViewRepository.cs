using EmployeeBOApp.Models;

public interface IBGVViewRepository
{
    Task<(List<TicketingTable> Tickets, int CurrentPage, int TotalPages)> GetPaginatedTickets(
        string searchQuery,
        string statusFilter,
        string requestType,
        int page,
        int pageSize);

    Task<(bool Success, string Message, TicketingTable Ticket, string PM, string DM, string LatestBgvId)> SubmitTicket(
        int id,
        string bgvId,
        string empId,
        string approvedBy);

    List<TicketingTable> GetFilteredTickets(string searchQuery, string statusFilter);
}
