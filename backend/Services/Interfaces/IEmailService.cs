namespace TransitHub.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string otp, string userName);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
        Task<bool> SendBookingConfirmationEmailAsync(string toEmail, string userName, string bookingReference, string trainName, decimal amount);
        Task<bool> SendBookingCancellationEmailAsync(string toEmail, string userName, string bookingReference, string trainName, decimal refundAmount);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName);
    }
}