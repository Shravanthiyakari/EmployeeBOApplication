using System;
using System.Collections.Generic;

namespace EmployeeBOApp.Models;

public partial class ProjectInformation
{
    public string ProjectId { get; set; } = null!;

    public string DepartmentID { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public string? ShortProjectName { get; set; }

    public string? Pm { get; set; }

    public string? Dm { get; set; }

    public string? PmemailId { get; set; }

    public string? DmemailId { get; set; }

    public virtual ICollection<EmployeeInformation> EmployeeInformations { get; set; } = new List<EmployeeInformation>();
}
