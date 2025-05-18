using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeBOApp.Models;

public partial class TicketingTable
{
    public int TicketingId { get; set; }

    public string? EmpId { get; set; }
    public string? DepartmentID { get; set; }
    public DateTime? EndDate { get; set; }

    [NotMapped]
    public string? EmpName { get; set; } // ✨ NotMapped ensures it's not in DB

    [NotMapped]
    public string? ProjectId { get; set; } // ✨ NotMapped ensures it's not in DB
    [NotMapped]
    public DateTime? StartDate { get; set; } // ✨ NotMapped ensures it's not in DB

    public string? RequestedBy { get; set; }

    public string? RequestType { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? RequestedDate { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public string? Comments { get; set; }

    public string? Status { get; set; }

    public virtual EmployeeInformation? Emp { get; set; }

   
}