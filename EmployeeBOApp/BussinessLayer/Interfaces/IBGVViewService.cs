using EmployeeBOApp.Models;

namespace EmployeeBOApp.BusinessLayer.Interfaces
{
    public interface IBGVViewService
    {
        Task<(List<TicketingTable> Tickets, int CurrentPage, int TotalPages)> GetPaginatedTicketsAsync(string searchQuery, string requestType, int page, int pageSize);
        Task<(bool Success, string Message)> ProcessBGVSubmission(int id, string bgvId, string empId, string empName, string approvedBy);
        (byte[] Content, string ContentType, string FileName) ExportTicketsToExcel(string searchQuery);
    }
}
