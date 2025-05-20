using System.ComponentModel.DataAnnotations;

namespace EmployeeBOApp.Models
{
    public partial class Login
    {

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(centific\.com)$",
        ErrorMessage = "Email must be a valid email address.")]
        public string EmailId { get; set; } = null!;

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = null!;

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = null!;
    }

}