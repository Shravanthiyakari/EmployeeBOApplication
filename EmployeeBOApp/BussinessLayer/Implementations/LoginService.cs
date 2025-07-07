using EmployeeBOApp.BusinessLayer.Interfaces;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;

namespace EmployeeBOApp.BusinessLayer.Services
{
    public class LoginService : ILoginService
    {
        private readonly ILoginRepository _loginRepository;

        public LoginService(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public async Task<Login> ValidateUserAsync(string emailId)
        {
            var user = await _loginRepository.GetLoginByEmailAsync(emailId);

            if (user != null && (user.Role == "PM" || user.Role == "DM" || user.Role == "GDO" || user.Role == "HR"))
                return user;

            return null!;
        }
    }
}
