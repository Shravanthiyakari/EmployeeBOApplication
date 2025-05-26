using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeBOApp.Data;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using System.Security.Claims;
using EmployeeBOApp.EmailContent;
using EmployeeBOApp.EmailServices;
using EmployeeBOApp;
using EmployeeBOApp.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

[Authorize]
public class BGVViewController : Controller
{
    private readonly EmployeeDatabaseContext _context;
    private readonly IEmailService _emailService;

    public BGVViewController(EmployeeDatabaseContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [Authorize]
    public async Task<IActionResult> BGVIndex(string searchQuery, string requestType = "BGV", int page = 1)
    {
        int pageSize = 10;

        var ticketsQuery = _context.TicketingTables
        .Include(t => t.Emp)
            .ThenInclude(e => e.BgvMap)
        .Where(t => t.RequestType == requestType);

        if (!string.IsNullOrEmpty(searchQuery))
        {
            ticketsQuery = ticketsQuery.Where(t =>
                t.Emp.EmpName.Contains(searchQuery) ||
                t.Emp.EmpId.Contains(searchQuery)   ||
               t.Emp.BgvMap.BGVId.Contains(searchQuery));
        }

        var totalItems = await ticketsQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var tickets = await ticketsQuery
            .OrderByDescending(t => t.TicketingId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;
        ViewData["SearchQuery"] = searchQuery;
        ViewData["RequestType"] = requestType;

        return View("BGVIndex", tickets);
    }

    [HttpPost]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> SubmitTicket(int id, string bgvId, string empId, string empName)
    {
        var ticket = await _context.TicketingTables
                                   .Include(t => t.Emp)
                                   .FirstOrDefaultAsync(t => t.TicketingId == id);
        
        
        var latestBgv = await _context.Bgvmaps
                             .Where(b => b.EmpId == empId)
                             .OrderByDescending(b => b.Date)
                             .FirstOrDefaultAsync();

        if (ticket != null)
        {
            var employee = await _context.EmployeeInformations
            .Include(e => e.BgvMap)
            .FirstOrDefaultAsync(e => e.EmpId == empId);

            if (latestBgv != null)
            {
                // Step 2: Get employee including its mapped BGV

                if (employee != null && employee.BgvMap != null)
                {
                    if (string.IsNullOrEmpty(employee.BgvMap.BGVId))
                    {
                        employee.BgvMap.BGVId = latestBgv.BGVId;
                        employee.BgvMap.BGVMappingId = latestBgv.BGVMappingId;

                        _context.Update(employee.BgvMap); // Optional if already tracked
                        await _context.SaveChangesAsync();
                    }
                }
            }
            else
            {
                    var bgvEntry = new Bgvmap
                    {
                        EmpId = empId,
                        BGVId = bgvId,
                        Date = DateTime.Now
                    };

                    _context.Bgvmaps.Add(bgvEntry);
                await _context.SaveChangesAsync();

                employee.BGVMappingId = bgvEntry.BGVMappingId;
                _context.Update(employee);
                await _context.SaveChangesAsync();
            }

                // Update ticket only for status/approval
            ticket.Status = "Closed";
            ticket.ApprovedBy = User.Identity?.Name;
            ticket.ApprovedDate = DateTime.Now;
            if(latestBgv!=null)
            {
                ticket.BGVId = latestBgv.BGVId;
            }
            else
            {
                ticket.BGVId = bgvId;
            }
            await _context.SaveChangesAsync();
        }

        var pm = ticket!.RequestedBy;
        string ccEmails = User.Identity?.Name ?? "";

        var DmName = await _context.ProjectInformations
            .Where(p => p.PmemailId == pm)
            .Select(p => p.DmemailId)
            .FirstOrDefaultAsync();
        var status = "Completed";
        string subject = $"BGV ID Request completed for - {empName} - {empId}";

        string finalBody = EmailContentForBGVID.EmailContentBGVID
            .Replace("{EMP_ID}", empId)
            .Replace("{EMP_NAME}", empName)
            .Replace("{BGV_ID}", bgvId)
            .Replace("{Status}", status)
            .Replace("{HR_NAME}", "HR-Team");

        try
        {
            await _emailService.SendEmailAsync(
                new List<string> { pm, DmName },
                subject,
                finalBody,
                true,
                ccEmails: new List<string> { ticket.ApprovedBy! }
            );
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"BGV details saved, but email failed: {ex.Message}";
            return RedirectToAction("BGVIndex");
        }

        TempData["Message"] = "BGV details saved, email sent successfully. For modification reach out HR-TEAM";
        return RedirectToAction("BGVIndex");
    }

    [HttpPost]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> DeleteRequest(int id)
    {
        var ticket = await _context.TicketingTables
                              .Include(t => t.Emp)
                              .FirstOrDefaultAsync(t => t.TicketingId == id);

        if (ticket != null && ticket.Emp != null)
        {
            // Step 1: Get EmpId from the ticket
            var empId = ticket.Emp.EmpId;

            // Step 2: Find matching employee in EmployeeInformation table
            var employee = await _context.EmployeeInformations
                                         .FirstOrDefaultAsync(e => e.EmpId == empId);

            // Step 3: If found, delete from EmployeeInformation
            if (employee != null)
            {

                var pm = ticket!.RequestedBy;
                string ccEmails = User.Identity?.Name ?? "";

                var DmName = await _context.ProjectInformations
                    .Where(p => p.PmemailId == pm)
                    .Select(p => p.DmemailId)
                    .FirstOrDefaultAsync();
                string status = "Deleted";
                string subject = $"Deleted BGV Request for - {employee.EmpName} - {employee.EmpId}";

                string finalBody = EmailContentForBGVID.EmailContentBGVID
                    .Replace("{EMP_ID}", employee.EmpId)
                    .Replace("{EMP_NAME}", employee.EmpName)
                    .Replace("{BGV_ID}", employee.BgvMap.BGVId)
                    .Replace("{Status}", status)
                    .Replace("{HR_NAME}", "HR-Team");

                try
                {
                    await _emailService.SendEmailAsync(
                        new List<string> { pm, DmName },
                        subject,
                        finalBody,
                        true,
                        ccEmails: new List<string> { ccEmails }
                    );
                }
                catch (Exception ex)
                {
                    return RedirectToAction("BGVIndex");
                }

                _context.EmployeeInformations.Remove(employee);

                // Step 4: Delete from TicketingTable
                _context.TicketingTables.Remove(ticket);

                // Save changes for both deletions
                await _context.SaveChangesAsync();
            }

        }
        return RedirectToAction("BGVIndex");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExportToExcel(string searchQuery)
    {
        // 1. Filter tickets based on searchQuery
        var filteredTickets = _context.TicketingTables
            .Include(t => t.Emp)
            .Where(t => t.RequestType == "BGV" &&
             (string.IsNullOrEmpty(searchQuery) ||
                     t.Emp!.EmpId.Contains(searchQuery) ||
                     t.Emp!.EmpName.Contains(searchQuery) ||
                     t.Emp!.BgvMap!.BGVId.Contains(searchQuery)))
        .ToList();

        // 2. Generate Excel file from filteredTickets (using ClosedXML)
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("BGV Tickets");

            // Add headers
            worksheet.Cell(1, 1).Value = "Emp ID";
            worksheet.Cell(1, 2).Value = "Emp Name";
            worksheet.Cell(1, 3).Value = "BGV ID";
            worksheet.Cell(1, 4).Value = "Ticket Status";

            int row = 2;
            foreach (var ticket in filteredTickets)
            {
                worksheet.Cell(row, 1).Value = ticket.Emp?.EmpId ?? "";
                worksheet.Cell(row, 2).Value = ticket.Emp?.EmpName ?? "";
                worksheet.Cell(row, 3).Value = ticket.Emp?.BgvMap.BGVId ?? "";
                worksheet.Cell(row, 4).Value = ticket.Status ?? "";
                row++;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();

                return File(content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "BGV_Tickets.xlsx");
            }
        }
    }

}

