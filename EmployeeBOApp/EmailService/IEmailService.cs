using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeBOApp
{
    public interface IEmailService
    {
        Task SendEmailAsync(IEnumerable<string> toEmails, string subject, string body, bool isHtml = false, IEnumerable<string> ccEmails = null);
    }
}