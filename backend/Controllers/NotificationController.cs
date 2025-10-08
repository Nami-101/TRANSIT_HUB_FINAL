using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TransitHub.Data;
using TransitHub.Services;

namespace TransitHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly TransitHubDbContext _context;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            INotificationService notificationService,
            TransitHubDbContext context,
            ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetNotifications([FromQuery] int limit = 10)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Ok(new List<object>());
                }

                var notifications = await _notificationService.GetUserNotificationsAsync(currentUser.UserID, limit);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return Ok(new List<object>());
            }
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult> GetUnreadCount()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Ok(new { count = 0 });
                }

                var count = await _notificationService.GetUnreadCountAsync(currentUser.UserID);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return Ok(new { count = 0 });
            }
        }

        [HttpPost("mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return Unauthorized();
                }

                await _notificationService.MarkAllAsReadAsync(currentUser.UserID);
                return Ok(new { message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notifications as read");
                return StatusCode(500, "An error occurred while marking notifications as read");
            }
        }

        private async Task<Models.User?> GetCurrentUserAsync()
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