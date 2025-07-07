using global::EmployeeBOApp.Models;
using System.Collections.Generic;

    namespace EmployeeBOApp.Repositories.Interfaces
    {
        public interface IEmployeeRepository
        {
            List<ProjectInformation> GetProjectsByUser(string email, string role);
            List<EmployeeInformation> GetEmployees(string selectedProjectId);
        }
    }


