using System.ComponentModel.DataAnnotations;

namespace TransitHub.Models.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}