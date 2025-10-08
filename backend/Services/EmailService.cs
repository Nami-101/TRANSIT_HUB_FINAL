using System.Net;
using System.Net.Mail;
using TransitHub.Services.Interfaces;

namespace TransitHub.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp, string userName)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"];
                var senderPassword = emailSettings["SenderPassword"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

                _logger.LogInformation("Attempting to send OTP email to {Email} using SMTP {Server}:{Port}", toEmail, smtpServer, smtpPort);
                _logger.LogInformation("Sender email: {SenderEmail}, SSL: {EnableSsl}", senderEmail, enableSsl);

                // Validate email settings
                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogError("Email settings are missing. SenderEmail or SenderPassword is empty.");
                    return false;
                }

                if (string.IsNullOrEmpty(smtpServer))
                {
                    _logger.LogError("SMTP server is not configured.");
                    return false;
                }

                _logger.LogInformation("Creating SMTP client for {Server}:{Port}", smtpServer, smtpPort);

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = enableSsl,
                    Timeout = 30000 // 30 seconds timeout
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Transit Hub"),
                    Subject = "Verify Your Email - Transit Hub",
                    Body = GenerateOtpEmailBody(userName, otp),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                _logger.LogInformation("Sending email message...");
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("OTP email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending OTP email to {Email}. SMTP Error: {SmtpError}", toEmail, smtpEx.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}. Error: {ErrorMessage}", toEmail, ex.Message);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"];
                var senderPassword = emailSettings["SenderPassword"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail!, "Transit Hub"),
                    Subject = "Welcome to Transit Hub!",
                    Body = GenerateWelcomeEmailBody(userName),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Welcome email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
                return false;
            }
        }

        private static string GenerateOtpEmailBody(string userName, string otp)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; font-weight: bold; color: #3f51b5; }}
                        .otp-box {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0; }}
                        .otp {{ font-size: 32px; font-weight: bold; color: #3f51b5; letter-spacing: 5px; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; text-align: center; color: #666; font-size: 14px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>Transit Hub</div>
                        </div>
                        
                        <h2>Email Verification Required</h2>
                        <p>Hello {userName},</p>
                        <p>Thank you for registering with Transit Hub! To complete your registration, please verify your email address using the OTP below:</p>
                        
                        <div class='otp-box'>
                            <div class='otp'>{otp}</div>
                            <p><strong>This OTP will expire in 10 minutes</strong></p>
                        </div>
                        
                        <p>If you didn't create an account with Transit Hub, please ignore this email.</p>
                        
                        <div class='footer'>
                            <p>Best regards,<br>The Transit Hub Team</p>
                            <p>This is an automated email. Please do not reply.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public async Task<bool> SendBookingConfirmationEmailAsync(string toEmail, string userName, string bookingReference, string trainName, decimal amount)
        {
            try
            {
                _logger.LogInformation("Starting SendBookingConfirmationEmailAsync for {Email}, PNR: {PNR}", toEmail, bookingReference);
                
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"];
                var senderPassword = emailSettings["SenderPassword"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

                _logger.LogInformation("Email config - Server: {Server}, Port: {Port}, Sender: {Sender}, SSL: {SSL}", 
                    smtpServer, smtpPort, senderEmail, enableSsl);

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogError("Email settings missing - SenderEmail or SenderPassword is empty");
                    return false;
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail!, "Transit Hub"),
                    Subject = $"Booking Confirmed - {bookingReference}",
                    Body = GenerateBookingConfirmationEmailBody(userName, bookingReference, trainName, amount),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                _logger.LogInformation("Sending booking confirmation email via SMTP...");
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Booking confirmation email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send booking confirmation email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendBookingCancellationEmailAsync(string toEmail, string userName, string bookingReference, string trainName, decimal refundAmount)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"];
                var senderPassword = emailSettings["SenderPassword"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail!, "Transit Hub"),
                    Subject = $"Booking Cancelled - {bookingReference}",
                    Body = GenerateBookingCancellationEmailBody(userName, bookingReference, trainName, refundAmount),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Booking cancellation email sent to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send booking cancellation email to {Email}", toEmail);
                return false;
            }
        }

        private static string GenerateWelcomeEmailBody(string userName)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; font-weight: bold; color: #3f51b5; }}
                        .welcome-box {{ background-color: #e8f5e8; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; text-align: center; color: #666; font-size: 14px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>🚂 Transit Hub</div>
                        </div>
                        
                        <div class='welcome-box'>
                            <h2>🎉 Welcome to Transit Hub!</h2>
                        </div>
                        
                        <p>Hello {userName},</p>
                        <p>Your email has been successfully verified! You can now enjoy all the features of Transit Hub:</p>
                        
                        <ul>
                            <li>🚂 Book train tickets</li>
                            <li>✈️ Book flight tickets</li>
                            <li>📱 Manage your bookings</li>
                            <li>🎫 View booking history</li>
                            <li>💳 Secure payment processing</li>
                            <li>📧 Email notifications</li>
                        </ul>
                        
                        <p>Start exploring now and make your travel planning easier than ever!</p>
                        
                        <div class='footer'>
                            <p>Best regards,<br>The Transit Hub Team</p>
                            <p>This is an automated email. Please do not reply.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string GenerateBookingConfirmationEmailBody(string userName, string bookingReference, string trainName, decimal amount)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; font-weight: bold; color: #3f51b5; }}
                        .success-box {{ background-color: #d4edda; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .booking-details {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; text-align: center; color: #666; font-size: 14px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>🚂 Transit Hub</div>
                        </div>
                        
                        <div class='success-box'>
                            <h2 style='color: #155724; margin: 0;'>✅ Booking Confirmed!</h2>
                        </div>
                        
                        <p>Dear {userName},</p>
                        <p>Great news! Your train booking has been confirmed successfully.</p>
                        
                        <div class='booking-details'>
                            <h3>Booking Details:</h3>
                            <p><strong>PNR:</strong> {bookingReference}</p>
                            <p><strong>Train:</strong> {trainName}</p>
                            <p><strong>Total Amount:</strong> ₹{amount:F2}</p>
                        </div>
                        
                        <p>Please keep this email for your records. You can also view your booking details in your Transit Hub account.</p>
                        
                        <div class='footer'>
                            <p>Thank you for choosing Transit Hub!</p>
                            <p>Best regards,<br>The Transit Hub Team</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName)
        {
            try
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"];
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var senderEmail = emailSettings["SenderEmail"];
                var senderPassword = emailSettings["SenderPassword"];
                var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogError("Email settings missing for password reset email");
                    return false;
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail!, "Transit Hub"),
                    Subject = "Reset Your Password - Transit Hub",
                    Body = GeneratePasswordResetEmailBody(userName, resetLink),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Password reset email sent to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
                return false;
            }
        }

        private static string GeneratePasswordResetEmailBody(string userName, string resetLink)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; font-weight: bold; color: #3f51b5; }}
                        .reset-box {{ background-color: #fff3cd; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .reset-button {{ display: inline-block; background-color: #3f51b5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; text-align: center; color: #666; font-size: 14px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>🚂 Transit Hub</div>
                        </div>
                        
                        <div class='reset-box'>
                            <h2 style='color: #856404; margin: 0;'>🔐 Password Reset Request</h2>
                        </div>
                        
                        <p>Hello {userName},</p>
                        <p>We received a request to reset your password for your Transit Hub account. If you made this request, click the button below to reset your password:</p>
                        
                        <div style='text-align: center;'>
                            <a href='{resetLink}' class='reset-button'>Reset Password</a>
                        </div>
                        
                        <p><strong>This link will expire in 1 hour for security reasons.</strong></p>
                        
                        <p>If you didn't request a password reset, please ignore this email. Your password will remain unchanged.</p>
                        
                        <p>If the button doesn't work, copy and paste this link into your browser:</p>
                        <p style='word-break: break-all; color: #3f51b5;'>{resetLink}</p>
                        
                        <div class='footer'>
                            <p>Best regards,<br>The Transit Hub Team</p>
                            <p>This is an automated email. Please do not reply.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private static string GenerateBookingCancellationEmailBody(string userName, string bookingReference, string trainName, decimal refundAmount)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }}
                        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                        .header {{ text-align: center; margin-bottom: 30px; }}
                        .logo {{ font-size: 24px; font-weight: bold; color: #3f51b5; }}
                        .cancel-box {{ background-color: #f8d7da; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .refund-details {{ background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
                        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; text-align: center; color: #666; font-size: 14px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <div class='logo'>🚂 Transit Hub</div>
                        </div>
                        
                        <div class='cancel-box'>
                            <h2 style='color: #721c24; margin: 0;'>❌ Booking Cancelled</h2>
                        </div>
                        
                        <p>Dear {userName},</p>
                        <p>Your train booking has been successfully cancelled.</p>
                        
                        <div class='refund-details'>
                            <h3>Cancellation Details:</h3>
                            <p><strong>PNR:</strong> {bookingReference}</p>
                            <p><strong>Train:</strong> {trainName}</p>
                            <p><strong>Refund Amount:</strong> ₹{refundAmount:F2}</p>
                        </div>
                        
                        <p>The refund will be processed within 5-7 business days to your original payment method.</p>
                        
                        <div class='footer'>
                            <p>If you have any questions, please contact our support team.</p>
                            <p>Best regards,<br>The Transit Hub Team</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}