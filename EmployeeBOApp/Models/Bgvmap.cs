using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace EmployeeBOApp.Models
{
    public class Bgvmap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BGVMappingId { get; set; }

        public string? EmpId { get; set; }

        [Required]
        [StringLength(50)] // Adjust size as needed
        public string BGVId { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        public virtual ICollection<EmployeeInformation> EmployeeInformations { get; set; } = new List<EmployeeInformation>();
    }
}
