using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeBOApp.Data;

using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using System.Security.Claims;

public class BGVViewController : Controller
{
    private readonly EmployeeDatabaseContext _context;

    public BGVViewController(EmployeeDatabaseContext context)
    {
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> BGVIndex(string searchQuery, string requestType = "BGV", int page = 1)
    {
        int pageSize = 10;

        var ticketsQuery = _context.TicketingTables
            .Include(t => t.Emp)
            .Where(t => t.RequestType == requestType);

        if (!string.IsNullOrEmpty(searchQuery))
        {
            ticketsQuery = ticketsQuery.Where(t =>
                t.Emp.EmpName.Contains(searchQuery) ||
                t.Emp.EmpId.Contains(searchQuery) ||
                t.BgvId.Contains(searchQuery));
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
    [Authorize(Roles = "PM,HR")]
    public async Task<IActionResult> EditTicket(int id, string bgvId, string empId, string empName)
    {
        var ticket = await _context.TicketingTables
                                   .Include(t => t.Emp)
                                   .FirstOrDefaultAsync(t => t.TicketingId == id);

        if (ticket != null)
        {
            if (User.IsInRole("PM"))
            {
                if (ticket.Emp != null)
                {
                    ticket.Emp.EmpId = empId;
                    ticket.Emp.EmpName = empName;
                }
            }

            if (User.IsInRole("HR"))
            {
                ticket.BgvId = bgvId;
                ticket.Status = "InProgress";
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction("BGVIndex");
    }

    [HttpPost]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> SubmitTicket(int id)
    {
        var ticket = await _context.TicketingTables.FirstOrDefaultAsync(t => t.TicketingId == id);

        if (ticket != null)
        {
            ticket.Status = "Closed";
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("BGVIndex");
    }

    [HttpPost]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> DeleteRequest(int id)
    {
        var ticket = await _context.TicketingTables
                                   .Include(t => t.Emp)
                                   .FirstOrDefaultAsync(t => t.TicketingId == id);

        if (ticket != null)
        {
            _context.TicketingTables.Remove(ticket);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("BGVIndex");
    }

    //[HttpPost]
    //public async Task<IActionResult> ExportToExcel(string searchQuery, string requestType)
    //{

    //    return RedirectToAction("BGVIndex", new { searchQuery, requestType });
    //}
    [HttpPost]
    public IActionResult ExportToExcel(string searchQuery)
    {
        // Retrieve data based on searchQuery and role-based filtering
        var userEmail = User.Identity?.Name;
        var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        var ticketsQuery = _context.TicketingTables
            .Include(t => t.Emp)
            .ThenInclude(e => e!.Project)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchQuery))
        {
            ticketsQuery = ticketsQuery.Where(t =>
                t.Emp!.EmpId.Contains(searchQuery) ||
                t.Emp.EmpName.Contains(searchQuery)
            );
        }

        var result = ticketsQuery.ToList();

        // Create an Excel file using ClosedXML
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Requests");

            // Define headers
            worksheet.Cell(1, 1).Value = "Emp ID";
            worksheet.Cell(1, 2).Value = "Emp Name";
            worksheet.Cell(1, 3).Value = "BgvId";
            worksheet.Cell(1, 3).Value = "Status";



            // Populate rows with data
            for (int i = 0; i < result.Count; i++)
            {
                var ticket = result[i];
                var emp = ticket.Emp;
                var project = emp?.Project;

                worksheet.Cell(i + 2, 1).Value = emp?.EmpId;
                worksheet.Cell(i + 2, 2).Value = emp?.EmpName;
                worksheet.Cell(i + 2, 3).Value = ticket.BgvId;
                worksheet.Cell(i + 2, 7).Value = ticket.Status;
            }

            // Set the content type and file name for the response
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var fileName = "BGVViewRequests.xlsx";
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }

}
