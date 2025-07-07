using EmployeeBOApp.BusinessLayer.Interfaces;
using EmployeeBOApp.Models;
using EmployeeBOApp.Repositories.Interfaces;
using System.Collections.Generic;

namespace EmployeeBOApp.BusinessLayer.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public List<ProjectInformation> GetUserProjects(string email, string role)
        {
            return _employeeRepository.GetProjectsByUser(email, role);
        }

        public List<EmployeeInformation> GetEmployees(string selectedProjectId)
        {
            return _employeeRepository.GetEmployees(selectedProjectId);
        }
    }
}
