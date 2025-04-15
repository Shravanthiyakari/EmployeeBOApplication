using EmployeeBOApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmployeeBOApp.Controllers
{
    public class ViewController : Controller
    {
        private readonly EmployeeDatabaseContext _context;

        public ViewController(EmployeeDatabaseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchQuery)
        {
            var userEmail = User.Identity?.Name;
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var ticketsQuery = _context.TicketingTables
                .Include(t => t.Emp)
                    .ThenInclude(e => e.Project)
                .AsQueryable();

            // Role-based filtering
            if (userRoles.Contains("GDO"))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Status == "InProgress");
            }
            else if (userRoles.Contains("GeneralManager"))
            {
                ticketsQuery = ticketsQuery.Where(t => t.Status == "Approved" || t.Status == "Closed");
            }
            else
            {
                ticketsQuery = ticketsQuery.Where(t => t.RequestedBy == userEmail);
            }

            // Search
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                ticketsQuery = ticketsQuery.Where(t =>
                    (t.Emp != null && (
                        t.Emp.EmpId.ToLower().Contains(searchQuery) ||
                        t.Emp.EmpName.ToLower().Contains(searchQuery) ||
                        (t.Emp.Project != null && (
                            t.Emp.Project.ShortProjectName!.ToLower().Contains(searchQuery) ||
                            t.Emp.Project.ProjectName.ToLower().Contains(searchQuery) ||
                            t.Emp.Project.Pm!.ToLower().Contains(searchQuery)
                        ))
                    )) ||
                    (t.Status != null && t.Status.ToLower().Contains(searchQuery)) ||
                    (t.RequestType != null && t.RequestType.ToLower().Contains(searchQuery))
                );

            }

            var result = await ticketsQuery.ToListAsync();
            ViewData["SearchQuery"] = searchQuery;
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> CloseRequest(int id)
        {
            var ticket = await _context.TicketingTables.FindAsync(id);
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (ticket != null && ticket.Status == "InProgress" && userRoles.Contains("GDO"))
            {
                ticket.Status = "Closed";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var ticket = await _context.TicketingTables.FindAsync(id);

            if (ticket != null && ticket.Status == "Open" && ticket.RequestedBy == User.Identity?.Name)
            {
                _context.TicketingTables.Remove(ticket);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var ticket = await _context.TicketingTables.FindAsync(id);
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (ticket != null && ticket.Status == "InProgress" && userRoles.Contains("GDO"))
            {
                ticket.Status = "Rejected";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var ticket = await _context.TicketingTables.FindAsync(id);
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (ticket != null && ticket.Status == "InProgress" && userRoles.Contains("GDO"))
            {
                ticket.Status = "Approved";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

       
    }
}
