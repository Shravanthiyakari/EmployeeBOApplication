using EmployeeBOApp.Data;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeBOApp.Repositories.Implementations
{
    public class BGVRepository : IBGVRepository
    {
        private readonly EmployeeDatabaseContext _context;

        public BGVRepository(EmployeeDatabaseContext context)
        {
            _context = context;
        }

        public async Task<object> CheckExistingBGV(string empId)
        {
            var employee = await _context.EmployeeInformations
                .Include(e => e.BgvMap)
                .FirstOrDefaultAsync(e => e.EmpId == empId);

            var ticket = await _context.TicketingTables
                .FirstOrDefaultAsync(e => e.EmpId == empId);

            var employeeWithBgv = await _context.TicketingTables
                .Where(t => t.RequestType == "Deallocation")
                .Join(_context.EmployeeInformations,
                      ticket => ticket.EmpId,
                      emp => emp.EmpId,
                      (ticket, emp) => new { ticket, emp })
                .Where(joined => joined.emp.Deallocation == true &&
                                 joined.emp.BGVMappingId != null &&
                                 joined.emp.ProjectId != null)
                .Select(joined => new
                {
                    EmpID = joined.emp.EmpId,
                    joined.emp.EmpName,
                    joined.emp.ProjectId,
                    joined.emp.BgvMap!.BGVId
                })
                .FirstOrDefaultAsync();

            bool projectStatus = ticket?.BGVId != null && ticket.Status == "Closed" && employee?.ProjectId == null;
            bool deallocationStatus = employeeWithBgv != null;

            return new
            {
                exists = employee?.BgvMap != null,
                bgvid = employee?.BgvMap?.BGVId,
                projectId = employee?.ProjectId,
                ExpirationDate = employee?.BgvMap?.Date.AddYears(1).ToString("yyyy-MM-dd"),
                projectStatus,
                deallocationStatus,
                empbgvproject = employeeWithBgv?.ProjectId,
                empbgvid = employeeWithBgv?.BGVId
            };
        }

        public async Task<(bool Success, string Message)> InitiateBGV(EmployeeInformation model, bool confirm, string requestedBy)
        {
            var existingEmployee = await _context.EmployeeInformations
                .Include(e => e.BgvMap)
                .FirstOrDefaultAsync(e => e.EmpId == model.EmpId);

            if (existingEmployee != null)
            {
                if (existingEmployee.BgvMap == null)
                {
                    _context.Bgvmaps.Add(new Bgvmap
                    {
                        EmpId = model.EmpId,
                        BGVId = string.Empty,
                        Date = DateTime.Now
                    });

                    await _context.SaveChangesAsync();
                    return (true, "BGV created for existing employee.");
                }
                else if (!confirm)
                {
                    return (false, "Employee already has BGV. Confirm to update.");
                }
                else
                {
                    existingEmployee.BgvMap.Date = DateTime.Now;
                    existingEmployee.BgvMap.BGVId = Guid.NewGuid().ToString();
                    _context.Bgvmaps.Update(existingEmployee.BgvMap);
                    await _context.SaveChangesAsync();
                    return (true, "BGV updated successfully for existing employee.");
                }
            }
            else
            {
                _context.EmployeeInformations.Add(model);
                await _context.SaveChangesAsync();

                _context.Bgvmaps.Add(new Bgvmap
                {
                    EmpId = model.EmpId,
                    BGVId = string.Empty,
                    Date = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            // Add Ticket
            _context.TicketingTables.Add(new TicketingTable
            {
                EmpId = model.EmpId,
                RequestType = "BGV",
                Status = "Open",
                EndDate = DateTime.UtcNow,
                RequestedBy = requestedBy,
                RequestedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return (true, "New employee and BGV created successfully.");
        }

        public async Task<List<string>> GetHRCCEmails()
        {
            return await _context.Logins
                .Where(l => l.Role.Contains("HR"))
                .Select(l => l.EmailId)
                .ToListAsync();
        }

        public async Task<string> GetPMNameByEmail(string email)
        {
            return await _context.ProjectInformations
                .Where(p => p.PmemailId == email)
                .Select(p => p.Pm)
                .FirstOrDefaultAsync() ?? "";
        }
    }
}
