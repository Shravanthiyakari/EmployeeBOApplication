using ClosedXML.Excel;
using EmployeeBOApp;
using EmployeeBOApp.EmailContent;
using EmployeeBOApp.Models;

public class BGVViewService : IBGVViewService
{
    private readonly IBGVViewRepository _repo;
    private readonly IEmailService _emailService;

    public BGVViewService(IBGVViewRepository repo, IEmailService emailService)
    {
        _repo = repo;
        _emailService = emailService;
    }

    public async Task<(List<TicketingTable>, int, int)> GetPaginatedTicketsAsync(
        string searchQuery, string statusFilter, string requestType, int page, int pageSize)
    {
        return await _repo.GetPaginatedTickets(searchQuery, statusFilter, requestType, page, pageSize);
    }

    public async Task<(bool Success, string Message)> ProcessBGVSubmission(
        int id, string bgvId, string empId, string empName, string approvedBy)
    {
        var (success, message, ticket, pm, dm, latestBgvId) = await _repo.SubmitTicket(id, bgvId, empId, approvedBy);
        if (!success) return (false, message);

        try
        {
            string subject = $"BGV ID Request completed for - {empName} - {empId}";
            string finalBody = EmailContentForBGVID.EmailContentBGVID
                .Replace("{EMP_ID}", empId)
                .Replace("{EMP_NAME}", empName)
                .Replace("{BGV_ID}", latestBgvId)
                .Replace("{Status}", "Completed")
                .Replace("{HR_NAME}", "HR-Team");

            await _emailService.SendEmailAsync(
                new List<string> { pm, dm },
                subject,
                finalBody,
                true,
                new List<string> { approvedBy });

            return (true, "Email sent successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Email failed: {ex.Message}");
        }
    }

    public (byte[] Content, string ContentType, string FileName) ExportTicketsToExcel(string searchQuery, string statusFilter = "All")
    {
        var filteredTickets = _repo.GetFilteredTickets(searchQuery, statusFilter);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("BGV Tickets");
            worksheet.Cell(1, 1).Value = "Emp ID";
            worksheet.Cell(1, 2).Value = "Emp Name";
            worksheet.Cell(1, 3).Value = "BGV ID";
            worksheet.Cell(1, 4).Value = "Ticket Status";

            int row = 2;
            foreach (var ticket in filteredTickets)
            {
                worksheet.Cell(row, 1).Value = ticket.Emp?.EmpId ?? "";
                worksheet.Cell(row, 2).Value = ticket.Emp?.EmpName ?? "";
                worksheet.Cell(row, 3).Value = ticket.Emp?.BgvMap?.BGVId ?? "";
                worksheet.Cell(row, 4).Value = ticket.Status ?? "";
                row++;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return (
                    Content: stream.ToArray(),
                    ContentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileName: "BGV_Tickets.xlsx"
                );
            }
        }
    }
}
}
