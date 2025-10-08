using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TransitHub.Data;
using TransitHub.Models;
using TransitHub.Models.DTOs;

namespace TransitHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly TransitHubDbContext _context;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(TransitHubDbContext context, ILogger<ProfileController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ProfileDto>> GetProfile()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Unauthorized("User not found");
                }

                var profile = new ProfileDto
                {
                    UserID = currentUser.UserID,
                    Name = currentUser.Name,
                    Email = currentUser.Email,
                    Phone = currentUser.Phone,
                    Age = currentUser.Age,
                    AvatarId = currentUser.AvatarId,
                    IsSeniorCitizen = currentUser.IsSeniorCitizen,
                    CreatedAt = currentUser.CreatedAt
                };

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, "An error occurred while retrieving profile");
            }
        }

        [HttpPut]
        public async Task<ActionResult<UpdateProfileResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Unauthorized("User not found");
                }

                // Update user fields
                currentUser.Name = request.Name;
                currentUser.Phone = request.Phone;
                currentUser.Age = request.Age;
                currentUser.AvatarId = request.AvatarId;
                currentUser.IsSeniorCitizen = request.Age >= 58;
                currentUser.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var updatedProfile = new ProfileDto
                {
                    UserID = currentUser.UserID,
                    Name = currentUser.Name,
                    Email = currentUser.Email,
                    Phone = currentUser.Phone,
                    Age = currentUser.Age,
                    AvatarId = currentUser.AvatarId,
                    IsSeniorCitizen = currentUser.IsSeniorCitizen,
                    CreatedAt = currentUser.CreatedAt
                };

                return Ok(new UpdateProfileResponse
                {
                    Success = true,
                    Message = "Profile updated successfully",
                    Profile = updatedProfile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new UpdateProfileResponse
                {
                    Success = false,
                    Message = "An error occurred while updating profile"
                });
            }
        }

        [HttpGet("avatars")]
        public ActionResult<List<AvatarOption>> GetAvatarOptions()
        {
            var avatars = new List<AvatarOption>
            {
                new AvatarOption { Id = 1, Name = "Happy Face", Emoji = "😊", Color = "#4F46E5" },
                new AvatarOption { Id = 2, Name = "Cool Face", Emoji = "😎", Color = "#059669" },
                new AvatarOption { Id = 3, Name = "Wink Face", Emoji = "😉", Color = "#DC2626" },
                new AvatarOption { Id = 4, Name = "Smile Face", Emoji = "🙂", Color = "#7C3AED" },
                new AvatarOption { Id = 5, Name = "Grin Face", Emoji = "😁", Color = "#EA580C" },
                new AvatarOption { Id = 6, Name = "Laugh Face", Emoji = "😂", Color = "#0891B2" },
                new AvatarOption { Id = 7, Name = "Heart Eyes", Emoji = "😍", Color = "#BE185D" },
                new AvatarOption { Id = 8, Name = "Star Eyes", Emoji = "🤩", Color = "#CA8A04" },
                new AvatarOption { Id = 9, Name = "Thinking Face", Emoji = "🤔", Color = "#6B7280" },
                new AvatarOption { Id = 10, Name = "Peace Face", Emoji = "😌", Color = "#10B981" }
            };

            return Ok(avatars);
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var email = HttpContext.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
                     ?? HttpContext.User?.FindFirst("email")?.Value
                     ?? HttpContext.User?.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}