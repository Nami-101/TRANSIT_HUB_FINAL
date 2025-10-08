using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitHub.Models
{
    [Table("Users")]
    public class User : BaseEntity
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Range(0, 120)]
        public int Age { get; set; }

        [Required]
        public bool IsSeniorCitizen { get; set; } = false;

        [Required]
        public bool IsVerified { get; set; } = false;

        // Avatar selection (null = use first letter of name)
        public int? AvatarId { get; set; }

        // Password reset fields
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        // Navigation Properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<GmailVerificationToken> VerificationTokens { get; set; } = new List<GmailVerificationToken>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
    }
}