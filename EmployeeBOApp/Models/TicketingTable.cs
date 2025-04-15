using System;
using System.Collections.Generic;

namespace EmployeeBOApp.Models
{
    public partial class TicketingTable
    {
        public int TicketingId { get; set; }

        public string? EmpId { get; set; }
        public string? RequestType { get; set; }
        public DateTime? EndDate { get; set; }

        public string? RequestedBy { get; set; }

        public string? ApprovedBy { get; set; }

        public DateTime? RequestedDate { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public string? Comments { get; set; }

        public string? Status { get; set; }

        public virtual EmployeeInformation? Emp { get; set; }
    }
}