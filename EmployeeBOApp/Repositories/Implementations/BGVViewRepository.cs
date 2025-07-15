using EmployeeBOApp.Data;
using EmployeeBOApp.Models;
using Microsoft.EntityFrameworkCore;

public class BGVViewRepository : IBGVViewRepository
{
    private readonly EmployeeDatabaseContext _context;

    public BGVViewRepository(EmployeeDatabaseContext context)
    {
        _context = context;
    }

    public async Task<(List<TicketingTable> Tickets, int CurrentPage, int TotalPages)> GetPaginatedTickets(
        string searchQuery,
        string statusFilter,
        string requestType,
        int page,
        int pageSize)
    {
        var query = _context.TicketingTables
            .Include(t => t.Emp)
            .ThenInclude(e => e.BgvMap)
            .Where(t => t.RequestType == requestType);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(t =>
                t.Emp.EmpId.Contains(searchQuery) ||
                t.Emp.EmpName.Contains(searchQuery) ||
                t.Emp.BgvMap.BGVId.Contains(searchQuery));
        }

        if (statusFilter == "Open")
            query = query.Where(t => t.Status != "Closed");
        else if (statusFilter == "Closed")
            query = query.Where(t => t.Status == "Closed");

        var totalItems = await query.CountAsync();
        var tickets = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return (tickets, page, totalPages);
    }

    public List<TicketingTable> GetFilteredTickets(string searchQuery, string statusFilter = "All")
    {
        var query = _context.TicketingTables
            .Include(t => t.Emp)
            .ThenInclude(e => e!.BgvMap)
            .Where(t => t.RequestType == "BGV");

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(t =>
                t.Emp.EmpId.Contains(searchQuery) ||
                t.Emp.EmpName.Contains(searchQuery) ||
                t.Emp.BgvMap.BGVId.Contains(searchQuery));
        }

        if (statusFilter == "Open")
            query = query.Where(t => t.Status != "Closed");
        else if (statusFilter == "Closed")
            query = query.Where(t => t.Status == "Closed");

        return query.ToList();
    }

    public async Task<(bool, string, TicketingTable, string, string, string)> SubmitTicket(
        int id, string bgvId, string empId, string approvedBy)
    {
        var ticket = await _context.TicketingTables.Include(t => t.Emp).FirstOrDefaultAsync(t => t.TicketingId == id);
        var latestBgv = await _context.Bgvmaps.Where(b => b.EmpId == empId).OrderByDescending(b => b.Date).FirstOrDefaultAsync();
        var employee = await _context.EmployeeInformations.Include(e => e.BgvMap).FirstOrDefaultAsync(e => e.EmpId == empId);

        if (ticket == null || employee == null)
            return (false, "Ticket or employee not found", null, "", "", "");

        if (latestBgv != null && ticket.Status == "Open")
        {
            latestBgv.BGVId = bgvId;
            employee.BGVMappingId = latestBgv.BGVMappingId;
            _context.Bgvmaps.Update(latestBgv);
            _context.EmployeeInformations.Update(employee);
            await _context.SaveChangesAsync();
        }
        else
        {
            var bgvEntry = new Bgvmap { EmpId = empId, BGVId = bgvId, Date = DateTime.Now };
            _context.Bgvmaps.Add(bgvEntry);
            await _context.SaveChangesAsync();

            employee.BGVMappingId = bgvEntry.BGVMappingId;
            _context.EmployeeInformations.Update(employee);
            await _context.SaveChangesAsync();
        }

        ticket.Status = "Closed";
        ticket.ApprovedBy = approvedBy;
        ticket.ApprovedDate = DateTime.Now;
        ticket.BGVId = latestBgv?.BGVId ?? bgvId;
        await _context.SaveChangesAsync();

        var pm = ticket.RequestedBy;
        var dm = await _context.ProjectInformations
            .Where(p => p.PmemailId == pm)
            .Select(p => p.DmemailId)
            .FirstOrDefaultAsync();

        return (true, "Success", ticket, pm ?? "", dm ?? "", ticket.BGVId ?? bgvId);
    }
}
