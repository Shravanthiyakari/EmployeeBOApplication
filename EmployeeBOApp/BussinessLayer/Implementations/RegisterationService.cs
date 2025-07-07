using EmployeeBOApp.BusinessLayer.Interfaces;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;

namespace EmployeeBOApp.BusinessLayer.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IRegistrationRepository _registrationRepository;

        public RegistrationService(IRegistrationRepository registrationRepository)
        {
            _registrationRepository = registrationRepository;
        }

        public async Task<bool> RegisterUserAsync(Login model)
        {
            return await _registrationRepository.AddLoginAsync(model);
        }
    }
}
