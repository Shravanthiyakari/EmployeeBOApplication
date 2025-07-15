using EmployeeBOApp.Models;

public interface IBGVViewService
{
    Task<(List<TicketingTable> Tickets, int CurrentPage, int TotalPages)> GetPaginatedTicketsAsync(
        string searchQuery, string statusFilter, string requestType, int page, int pageSize);

    Task<(bool Success, string Message)> ProcessBGVSubmission(
        int id, string bgvId, string empId, string empName, string approvedBy);

    (byte[] Content, string ContentType, string FileName) ExportTicketsToExcel(string searchQuery, string statusFilter);
}
