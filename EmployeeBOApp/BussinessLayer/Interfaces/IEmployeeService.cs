using EmployeeBOApp.Models;
using System.Collections.Generic;

namespace EmployeeBOApp.BusinessLayer.Interfaces
{
    public interface IEmployeeService
    {
        List<ProjectInformation> GetUserProjects(string email, string role);
        List<EmployeeInformation> GetEmployees(string selectedProjectId);
    }
}
