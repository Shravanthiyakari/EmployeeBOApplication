using System;
using System.Collections.Generic;

namespace EmployeeBOApp.Models;

public partial class EmployeeInformation
{
    public string EmpId { get; set; } = null!;

    public string EmpName { get; set; } = null!;

    public bool Deallocation { get; set; }

    public string? ProjectId { get; set; }

    public virtual ProjectInformation? Project { get; set; }

    public virtual ICollection<TicketingTable> TicketingTables { get; set; } = new List<TicketingTable>();
}
