using Microsoft.AspNetCore.Mvc;
using EmployeeBOApp.Data; // Adjust based on your actual namespace
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Identity;
using EmployeeBOApp.Models;

public class BGVViewController : Controller
{
    private readonly EmployeeDatabaseContext _context;

    public BGVViewController(EmployeeDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> BGVIndex(string searchQuery = "", string requestType = "BGV", int page = 1)
    {
        string userEmail = User.Identity?.Name ?? "";
        IQueryable<TicketingTable> query;
        // This ensures only tickets requested by current user
        if (User.IsInRole("HR"))
        {
            query = _context.TicketingTables
            .Include(t => t.Emp)
                .ThenInclude(e => e!.Project)
            .Where(t =>
                t.RequestType == requestType);
        }
        else
        {
            query = _context.TicketingTables
    .Include(t => t.Emp)
        .ThenInclude(e => e!.Project)
    .Where(t =>
        t.RequestType == requestType &&
        t.Status == "Open" &&
        t.RequestedBy == userEmail);
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(t =>
                t.Emp.EmpName.Contains(searchQuery) ||
                t.Emp.EmpId.Contains(searchQuery) ||
                t.RequestType.Contains(searchQuery));
        }

        var tickets = await query.ToListAsync();

        return View(tickets);
    }
    [HttpPost]
    public async Task<IActionResult> EditTicket(int id, string bgvId)
    {
        var ticket = await _context.TicketingTables
                                   .Include(t => t.Emp)
                                   .FirstOrDefaultAsync(t => t.TicketingId == id);

        if (ticket != null)
        {
            // Set status to 'InProgress'
            ticket.Status = "InProgress";
            ticket.BgvId = bgvId; // Set the BGV ID to the new value

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("BGVIndex"); // Redirect back to the BGVIndex view
    }
    [HttpPost]
    public async Task<IActionResult> SubmitTicket(int id)
    {
        var ticket = await _context.TicketingTables
                                   .FirstOrDefaultAsync(t => t.TicketingId == id);

        if (ticket != null)
        {
            // Set status to 'Closed'
            ticket.Status = "Closed";

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("BGVIndex"); // Redirect back to the BGVIndex view
    }

}
