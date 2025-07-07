using EmployeeBOApp.BusinessLayer.Interfaces;
using EmployeeBOApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[Authorize]
public class BGVController : Controller
{
    private readonly IBGVService _bgvService;

    public BGVController(IBGVService bgvService)
    {
        _bgvService = bgvService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View("BGVForm");
    }

    [HttpGet]
    public async Task<JsonResult> CheckExistingBGV(string empId)
    {
        var result = await _bgvService.CheckExistingBGVAsync(empId);
        return Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> InitiateBGV(EmployeeInformation model, bool confirm = false)
    {
        var result = await _bgvService.InitiateBGVAsync(model, confirm, User.Identity?.Name ?? "");

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction("Index");
        }

        TempData["Message"] = result.Message;
        return RedirectToAction("Index");
    }
}
