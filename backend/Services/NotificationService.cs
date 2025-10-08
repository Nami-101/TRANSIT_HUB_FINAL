using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TransitHub.Data;
using TransitHub.Hubs;
using TransitHub.Models;

namespace TransitHub.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string title, string message, string type, int? relatedBookingId = null);
        Task<List<Notification>> GetUserNotificationsAsync(int userId, int limit = 10);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAllAsReadAsync(int userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly TransitHubDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            TransitHubDbContext context,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task CreateNotificationAsync(int userId, string title, string message, string type, int? relatedBookingId = null)
        {
            try
            {
                var notification = new Notification
                {
                    UserID = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Status = "Unread",
                    RelatedBookingID = relatedBookingId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send real-time notification via SignalR
                var signalRData = new
                {
                    id = notification.NotificationID,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type,
                    createdAt = notification.CreatedAt
                };
                
                // Get JWT user ID for SignalR group
                var user = await _context.Users.FindAsync(userId);
                var jwtUserId = user?.Email; // Use email as identifier since JWT uses email
                
                _logger.LogInformation("📡 Sending SignalR notification to User_{JwtUserId} (DB User {UserId}): {Data}", jwtUserId, userId, System.Text.Json.JsonSerializer.Serialize(signalRData));
                
                if (!string.IsNullOrEmpty(jwtUserId))
                {
                    await _hubContext.Clients.Group($"User_{jwtUserId}").SendAsync("NewNotification", signalRData);
                }

                _logger.LogInformation("Created notification for user {UserId}: {Title}", userId, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", userId);
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int limit = 10)
        {
            return await _context.Notifications
                .Where(n => n.UserID == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserID == userId && n.Status == "Unread");
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserID == userId && n.Status == "Unread")
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.Status = "Read";
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}