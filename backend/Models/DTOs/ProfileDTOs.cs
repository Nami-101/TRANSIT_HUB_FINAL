using System.ComponentModel.DataAnnotations;

namespace TransitHub.Models.DTOs
{
    public class ProfileDto
    {
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int Age { get; set; }
        public int? AvatarId { get; set; }
        public bool IsSeniorCitizen { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateProfileRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Range(1, 120)]
        public int Age { get; set; }

        public int? AvatarId { get; set; }
    }

    public class UpdateProfileResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ProfileDto? Profile { get; set; }
    }

    public class AvatarOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
}