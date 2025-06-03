using EmployeeBOApp;
using EmployeeBOApp.Data;
using EmployeeBOApp.EmailContent;
using EmployeeBOApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

[Authorize]
public class BGVController : Controller
{
    private readonly EmployeeDatabaseContext _context;
    private readonly IEmailService _emailService;

    public BGVController(EmployeeDatabaseContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View("BGVForm");
    }

    [HttpGet]
    public async Task<JsonResult> CheckExistingBGV(string empId)
    {
    var employee = await _context.EmployeeInformations
            .Include(e => e.BgvMap)
            .FirstOrDefaultAsync(e => e.EmpId == empId);
    if (employee == null)
        {
            return Json(new { success = true });
    }

        var ticket = await _context.TicketingTables
      .FirstOrDefaultAsync(e => e.EmpId == empId);
        bool deallocationStatus = false;
   
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
        joined.emp.BgvMap.BGVId
    })
    .FirstOrDefaultAsync();  // or ToListAsync() if you expect multiple

    bool projectStatus = false; // default value
    var empbgvproject = string.Empty;
    var empbgvid = string.Empty;

        if (ticket != null && ticket.BGVId != null && ticket.Status == "Closed")
        {
            if (employee != null)
            {
               projectStatus = employee.ProjectId == null;
            }
        }

        if(employeeWithBgv !=null)
        {
            deallocationStatus = true;
            empbgvproject = employeeWithBgv.ProjectId;
            empbgvid = employeeWithBgv.BGVId;



        }
        bool exists = employee.BgvMap != null;
        var bgv = employee.BgvMap;
      
        return Json(new
        {
         exists,
         bgvid = bgv != null ? bgv.BGVId : null,
         projectId = employee.ProjectId,
         ExpirationDate = employee?.BgvMap?.Date.AddYears(1),
         projectStatus,
         deallocationStatus,
         empbgvproject,
         empbgvid
        });
    }

    [HttpPost]
    public async Task<IActionResult> InitiateBGV(EmployeeInformation model, bool confirm = false)
    {
        if (!ModelState.IsValid)
        {
            return View("BGVForm", model);
        }

        var existingEmployee = await _context.EmployeeInformations
        .Include(e => e.BgvMap)
        .FirstOrDefaultAsync(e => e.EmpId == model.EmpId);

        if (existingEmployee != null && confirm)
        {
            // Optional: update timestamp or other logic
            var newBgv = new Bgvmap
            {
                EmpId = model.EmpId,
                BGVId = string.Empty, // Example ID
                Date = DateTime.Now
            };

            _context.Bgvmaps.Add(newBgv);
            await _context.SaveChangesAsync();

            // Link employee to new BGV
            await _context.SaveChangesAsync();

            TempData["Message"] = "BGV request updated successfully.";
        }
        else
        {
            // Save employee information to the database
            _context.EmployeeInformations.Add(model);
            _context.SaveChanges();

        }

        var ticket = new TicketingTable
        {
            EmpId = model.EmpId,
            RequestType = "BGV",
            Status = "Open",
            EndDate = DateTime.UtcNow,
            RequestedBy = User.Identity?.Name,
            RequestedDate = DateTime.Now
        };

        _context.TicketingTables.Add(ticket);
        await _context.SaveChangesAsync(); 

        var ccEmails = _context.Logins
       .Where(l => l.Role.Contains("HR"))
       .Select(l => l.EmailId)
       .ToList();
        string userEmail = User.Identity?.Name ?? "";

        var pmName = await _context.ProjectInformations
            .Where(p => p.PmemailId == userEmail)
            .Select(p => p.Pm)
            .FirstOrDefaultAsync();

        string actionLink = "<a class='action-link' href='https://localhost:7168/Login/Login'>Click here to take decision</a>";
        string subject = $"BGV Initiation Request for - {model.EmpName} - {model.EmpId}";

        string finalBody = EmailContentForBGV.EmailContentBGV
            .Replace("{EMP_ID}", model.EmpId)
            .Replace("{EMP_NAME}", model.EmpName)
            .Replace("{PM_NAME}", pmName)
            .Replace("{ACTION_LINK}", actionLink);

        try
        {
            await _emailService.SendEmailAsync(
                new List<string> { userEmail }, // Replace with actual recipient(s)
                subject,
                finalBody,
                true,
                ccEmails
            );
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"BGV details saved, but email failed: {ex.Message}";
            return RedirectToAction("Index");
        }

        TempData["Message"] = "BGV details saved and email sent successfully.For modifications reach out HR-TEAM";
        return RedirectToAction("Index");
    }
}
   
