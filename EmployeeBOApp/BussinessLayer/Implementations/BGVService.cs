using EmployeeBOApp.BusinessLayer.Interfaces;
using EmployeeBOApp.EmailContent;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeBOApp.BusinessLayer.Services
{
    public class BGVService : IBGVService
    {
        private readonly IBGVRepository _bgvRepo;
        private readonly IEmailService _emailService;

        public BGVService(IBGVRepository bgvRepo, IEmailService emailService)
        {
            _bgvRepo = bgvRepo;
            _emailService = emailService;
        }

        public async Task<object> CheckExistingBGVAsync(string empId)
        {
            return await _bgvRepo.CheckExistingBGV(empId);
        }

        public async Task<(bool Success, string Message)> InitiateBGVAsync(EmployeeInformation model, bool confirm, string userEmail)
        {
            try
            {
                var (status, message) = await _bgvRepo.InitiateBGV(model, confirm, userEmail);
                if (!status)
                    return (false, message);

                // Email logic
                var ccEmails = await _bgvRepo.GetHRCCEmails();
                var pmName = await _bgvRepo.GetPMNameByEmail(userEmail);

                string actionLink = "<a class='action-link' href='https://localhost:7168/Login/Login'>Click here to take decision</a>";
                string subject = $"BGV Initiation Request for - {model.EmpName} - {model.EmpId}";

                string finalBody = EmailContentForBGV.EmailContentBGV
                    .Replace("{EMP_ID}", model.EmpId)
                    .Replace("{EMP_NAME}", model.EmpName)
                    .Replace("{PM_NAME}", pmName)
                    .Replace("{ACTION_LINK}", actionLink);

                await _emailService.SendEmailAsync(
                    new List<string> { userEmail }, subject, finalBody, true, ccEmails
                );

                return (true, "BGV details saved and email sent successfully. For modifications reach out HR-TEAM");
            }
            catch (Exception ex)
            {
                return (false, $"BGV details saved, but email failed: {ex.Message}");
            }
        }
    }
}
