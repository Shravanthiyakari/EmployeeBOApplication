using EmployeeBOApp.Data;
using EmployeeBOApp.Models;
using Microsoft.AspNetCore.Mvc;

public class BGVController : Controller
{
    private readonly EmployeeDatabaseContext _context;

    public BGVController(EmployeeDatabaseContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View("BGVForm");
    }

    [HttpPost]
    public IActionResult InitiateBGV(EmployeeInformation model)
    {
        if (!ModelState.IsValid)
        {
            return View("BGVForm", model);
        }

        // Save employee information to the database
        _context.EmployeeInformations.Add(model);
        _context.SaveChanges();

        // Set a success message and redirect to the BGV form
        TempData["Message"] = "Employee details saved successfully.";
        return RedirectToAction("Index");
    }
}

