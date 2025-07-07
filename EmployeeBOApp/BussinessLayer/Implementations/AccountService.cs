using EmployeeBOApp.BusinessLayer.Interfaces;
using EmployeeBOApp.Repositories.Interfaces;

namespace EmployeeBOApp.BusinessLayer.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public void SubmitAccount()
        {
            _accountRepository.SubmitDetails();
        }
    }
}
