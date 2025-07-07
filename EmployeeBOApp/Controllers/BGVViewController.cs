using EmployeeBOApp.BusinessLayer.Interfaces;
using EmployeeBOApp.BussinessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class BGVViewController : Controller
{
    private readonly IBGVViewService _bgvViewService;

    public BGVViewController(IBGVViewService bgvViewService)
    {
        _bgvViewService = bgvViewService;
    }

    public async Task<IActionResult> BGVIndex(string searchQuery, string requestType = "BGV", int page = 1)
    {
        var viewModel = await _bgvViewService.GetPaginatedTicketsAsync(searchQuery, requestType, page, 10);
        ViewData["CurrentPage"] = viewModel.CurrentPage;
        ViewData["TotalPages"] = viewModel.TotalPages;
        ViewData["SearchQuery"] = searchQuery;
        ViewData["RequestType"] = requestType;

        return View("BGVIndex", viewModel.Tickets);
    }

    [HttpPost]
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> SubmitTicket(int id, string bgvId, string empId, string empName)
    {
        var result = await _bgvViewService.ProcessBGVSubmission(id, bgvId, empId, empName, User.Identity?.Name ?? "");

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
        }

        return RedirectToAction("BGVIndex");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ExportToExcel(string searchQuery)
    {
        var excelFile = _bgvViewService.ExportTicketsToExcel(searchQuery);
        return File(excelFile.Content, excelFile.ContentType, excelFile.FileName);
    }
}
