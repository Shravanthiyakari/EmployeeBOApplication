using EmployeeBOApp;
using EmployeeBOApp.Data;
using EmployeeBOApp.EmailContent;
using EmployeeBOApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    [HttpPost]
    public async Task<IActionResult> InitiateBGV(EmployeeInformation model)
    {
        if (!ModelState.IsValid)
        {
            return View("BGVForm", model);
        }

        // Save employee information to the database
        _context.EmployeeInformations.Add(model);
        _context.SaveChanges();

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
        await _context.SaveChangesAsync(); // Save the new ticket row

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

        TempData["Message"] = "BGV details saved and email sent successfully.";
        return RedirectToAction("Index");
    }
}
