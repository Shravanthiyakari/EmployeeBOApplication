using ClosedXML.Excel;
using EmployeeBOApp.Data;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeBOApp.Controllers
{
    [Authorize]
    public class ViewController : Controller
    {
        private readonly EmployeeDatabaseContext _context;
        private readonly IViewRepository _viewRepository;


        public ViewController(EmployeeDatabaseContext context, IViewRepository viewRepository)
        {
            _context = context;
            _viewRepository = viewRepository;
        }

        public async Task<IActionResult> Index(string searchQuery, string requestType, int page = 1)
        {
            var userEmail = User.Identity?.Name;
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var (tickets, totalItems) = await _viewRepository.GetFilteredTicketsAsync(
                searchQuery, requestType, userEmail, userRoles, page);

            ViewData["SearchQuery"] = searchQuery;
            ViewData["RequestType"] = requestType;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(totalItems / 10.0);

            return View(tickets);
        }
        [HttpPost]
        public async Task<IActionResult> ExportToExcel(string searchQuery, string requestType)
        {
            var userEmail = User.Identity?.Name;
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var result = await _viewRepository.GetTicketsForExportAsync(
                searchQuery, requestType, userEmail, userRoles);

            // Generate Excel using ClosedXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Requests");

                // Headers
                worksheet.Cell(1, 1).Value = "Emp ID";
                worksheet.Cell(1, 2).Value = "Emp Name";
                worksheet.Cell(1, 3).Value = "Request Type";
                worksheet.Cell(1, 4).Value = "Project Code";
                worksheet.Cell(1, 5).Value = "Project Name";
                worksheet.Cell(1, 6).Value = "PM";
                worksheet.Cell(1, 7).Value = "Status";

                // Rows
                for (int i = 0; i < result.Count; i++)
                {
                    var ticket = result[i];
                    var emp = ticket.Emp;
                    var project = emp?.Project;

                    worksheet.Cell(i + 2, 1).Value = emp?.EmpId;
                    worksheet.Cell(i + 2, 2).Value = emp?.EmpName;
                    worksheet.Cell(i + 2, 3).Value = ticket.RequestType;
                    worksheet.Cell(i + 2, 4).Value = project?.ShortProjectName;
                    worksheet.Cell(i + 2, 5).Value = project?.ProjectName;
                    worksheet.Cell(i + 2, 6).Value = project?.Pm;
                    worksheet.Cell(i + 2, 7).Value = ticket.Status;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var fileName = "ViewRequests.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> CloseRequest(int id)
        {
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var approvedBy = User.Identity?.Name;

            var (success, errorMessage) = await _viewRepository.CloseRequestAsync(id, approvedBy!, userRoles);

            if (!success)
            {
                return Json(new { success = false, message = errorMessage });
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var approvedBy = User.Identity?.Name;

            var (success, errorMessage) = await _viewRepository.ApproveRequestAsync(id, approvedBy!, userRoles);

            if (!success)
            {
                return Json(new { success = false, message = errorMessage });
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var requestedBy = User.Identity?.Name;
            if (string.IsNullOrEmpty(requestedBy))
                return Unauthorized();

            var (success, errorMessage) = await _viewRepository.DeleteRequestAsync(id, requestedBy);

            if (!success)
                return Json(new { success = false, message = errorMessage });

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
                return Unauthorized();

            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var (success, errorMessage) = await _viewRepository.RejectRequestAsync(id, userName, userRoles);

            if (!success)
                return Json(new { success = false, message = errorMessage });

            return RedirectToAction("Index");
        }
    }
}