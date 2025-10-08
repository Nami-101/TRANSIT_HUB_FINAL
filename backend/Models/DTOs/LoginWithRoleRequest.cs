using System.ComponentModel.DataAnnotations;

namespace TransitHub.Models.DTOs
{
    public class LoginWithRoleRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string SelectedRole { get; set; } = string.Empty;
    }
}