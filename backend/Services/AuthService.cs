using Microsoft.AspNetCore.Identity;
using TransitHub.Models.DTOs;
using TransitHub.Services.Interfaces;
using TransitHub.Repositories.Interfaces;
using TransitHub.Models;

namespace TransitHub.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IJwtService jwtService,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Invalid email or password");
                }

                if (!user.EmailConfirmed)
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Please verify your email before logging in");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        return ApiResponse<LoginResponse>.ErrorResult("Account is locked out");
                    }
                    return ApiResponse<LoginResponse>.ErrorResult("Invalid email or password");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var customUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                var actualName = customUser?.Name ?? user.Email ?? "";
                
                // Check if user is admin and requires role selection
                if (roles.Contains("Admin"))
                {
                    var loginResponse = new LoginResponse
                    {
                        RequiresRoleSelection = true,
                        AvailableRoles = new List<string> { "Admin", "User" },
                        User = new UserInfo
                        {
                            Id = user.Id,
                            Email = user.Email!,
                            FirstName = actualName,
                            LastName = "",
                            Roles = roles.ToList()
                        }
                    };
                    
                    return ApiResponse<LoginResponse>.SuccessResult(loginResponse, "Role selection required");
                }

                // Regular user login
                var token = await _jwtService.GenerateTokenAsync(user, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var regularLoginResponse = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    RequiresRoleSelection = false,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FirstName = actualName,
                        LastName = "",
                        Roles = roles.ToList()
                    }
                };

                _logger.LogInformation("User {Email} logged in successfully", request.Email);
                return ApiResponse<LoginResponse>.SuccessResult(regularLoginResponse, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", request.Email);
                return ApiResponse<LoginResponse>.ErrorResult("An error occurred during login");
            }
        }

        public async Task<ApiResponse<LoginResponse>> LoginWithRoleAsync(LoginWithRoleRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Invalid email or password");
                }

                if (!user.EmailConfirmed)
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Please verify your email before logging in");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        return ApiResponse<LoginResponse>.ErrorResult("Account is locked out");
                    }
                    return ApiResponse<LoginResponse>.ErrorResult("Invalid email or password");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var customUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                var actualName = customUser?.Name ?? user.Email ?? "";
                
                // Validate selected role
                if (!userRoles.Contains("Admin"))
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Role selection not available for this user");
                }

                if (request.SelectedRole != "Admin" && request.SelectedRole != "User")
                {
                    return ApiResponse<LoginResponse>.ErrorResult("Invalid role selected");
                }

                // Create token with selected role
                var tokenRoles = new List<string> { request.SelectedRole };
                var token = await _jwtService.GenerateTokenAsync(user, tokenRoles);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var loginResponse = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    RequiresRoleSelection = false,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FirstName = actualName,
                        LastName = "",
                        Roles = tokenRoles
                    }
                };

                _logger.LogInformation("User {Email} logged in successfully with role {Role}", request.Email, request.SelectedRole);
                return ApiResponse<LoginResponse>.SuccessResult(loginResponse, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during role-based login for user {Email}", request.Email);
                return ApiResponse<LoginResponse>.ErrorResult("An error occurred during login");
            }
        }

        public Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // In a real application, you would store refresh tokens in database
                // and validate them here. For now, we'll generate a new token
                // This is a simplified implementation

                return Task.FromResult(ApiResponse<LoginResponse>.ErrorResult("Refresh token functionality not implemented yet"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return Task.FromResult(ApiResponse<LoginResponse>.ErrorResult("An error occurred during token refresh"));
            }
        }

        public async Task<ApiResponse> LogoutAsync(string userId)
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User {UserId} logged out successfully", userId);
                return ApiResponse.SuccessResult("Logout successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", userId);
                return ApiResponse.ErrorResult("An error occurred during logout");
            }
        }

        public async Task<ApiResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Starting registration for user {Email}", request.Email);
                
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: User {Email} already exists", request.Email);
                    return ApiResponse.ErrorResult("User with this email already exists");
                }

                var user = new IdentityUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    EmailConfirmed = false // Require email verification
                };

                _logger.LogInformation("Creating user {Email} with Identity", request.Email);
                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("User creation failed for {Email}: {Errors}", request.Email, string.Join(", ", errors));
                    return ApiResponse.ErrorResult("Registration failed", errors);
                }

                // Add user to default role
                try
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    _logger.LogInformation("User {Email} added to 'User' role", request.Email);
                }
                catch (Exception roleEx)
                {
                    _logger.LogWarning(roleEx, "Could not add user {Email} to 'User' role", request.Email);
                }

                // Create custom User record with actual name
                var customUser = new User
                {
                    Name = $"{request.FirstName} {request.LastName}".Trim(),
                    Email = request.Email,
                    Phone = request.PhoneNumber ?? "",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                };

                await _unitOfWork.Users.AddAsync(customUser);
                await _unitOfWork.SaveChangesAsync();

                // Generate and send OTP
                var otp = GenerateOtp();
                var otpRecord = new EmailVerificationOtp
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    Otp = otp,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.EmailVerificationOtps.AddAsync(otpRecord);
                await _unitOfWork.SaveChangesAsync();

                // Send OTP email
                _logger.LogInformation("Attempting to send OTP email to {Email}", user.Email);
                var emailSent = await _emailService.SendOtpEmailAsync(user.Email!, otp, request.FirstName ?? "User");
                if (!emailSent)
                {
                    _logger.LogWarning("Failed to send OTP email to {Email}. Registration completed but email verification required.", user.Email);
                    // Don't fail registration if email fails - user can request resend
                }

                _logger.LogInformation("User {Email} registered successfully. OTP sent.", request.Email);
                return ApiResponse.SuccessResult("Registration successful. Please check your email for verification code.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Email}", request.Email);
                return ApiResponse.ErrorResult("An error occurred during registration");
            }
        }

        public async Task<ApiResponse> VerifyEmailAsync(VerifyEmailRequest request)
        {
            try
            {
                var otpRecord = await _unitOfWork.EmailVerificationOtps
                    .FirstOrDefaultAsync(o => o.Email == request.Email && o.Otp == request.Otp && !o.IsUsed);

                if (otpRecord == null)
                {
                    return ApiResponse.ErrorResult("Invalid or expired OTP");
                }

                if (otpRecord.ExpiresAt < DateTime.UtcNow)
                {
                    return ApiResponse.ErrorResult("OTP has expired");
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse.ErrorResult("User not found");
                }

                // Mark email as confirmed
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                // Mark OTP as used
                otpRecord.IsUsed = true;
                otpRecord.VerifiedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                // Get the actual user name for welcome email
                var customUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                var actualName = customUser?.Name ?? user.UserName ?? "User";
                
                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email!, actualName);

                _logger.LogInformation("Email verified successfully for user {Email}", request.Email);
                return ApiResponse.SuccessResult("Email verified successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for {Email}", request.Email);
                return ApiResponse.ErrorResult("An error occurred during email verification");
            }
        }

        public async Task<ApiResponse> ResendOtpAsync(ResendOtpRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return ApiResponse.ErrorResult("User not found");
                }

                if (user.EmailConfirmed)
                {
                    return ApiResponse.ErrorResult("Email is already verified");
                }

                // Generate new OTP
                var otp = GenerateOtp();
                var otpRecord = new EmailVerificationOtp
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    Otp = otp,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.EmailVerificationOtps.AddAsync(otpRecord);
                await _unitOfWork.SaveChangesAsync();

                // Send OTP email
                var emailSent = await _emailService.SendOtpEmailAsync(user.Email!, otp, user.UserName ?? "User");
                if (!emailSent)
                {
                    return ApiResponse.ErrorResult("Failed to send OTP email");
                }

                _logger.LogInformation("OTP resent successfully to {Email}", request.Email);
                return ApiResponse.SuccessResult("OTP sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending OTP to {Email}", request.Email);
                return ApiResponse.ErrorResult("An error occurred while resending OTP");
            }
        }

        public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string frontendUrl = "http://localhost:4200")
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    // Don't reveal if user exists or not for security
                    return ApiResponse.SuccessResult("If an account with that email exists, a password reset link has been sent.");
                }

                // Generate reset token
                var resetToken = Guid.NewGuid().ToString("N");
                var customUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                
                if (customUser != null)
                {
                    customUser.ResetToken = resetToken;
                    customUser.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // 1 hour expiry
                    await _unitOfWork.SaveChangesAsync();
                }

                // Create reset link using the actual frontend URL
                var resetLink = $"{frontendUrl}/auth/reset-password?token={resetToken}";
                _logger.LogInformation("Password reset requested for {Email}. Reset link: {ResetLink}", request.Email, resetLink);
                
                // In production, send actual email
                var actualName = customUser?.Name ?? "User";
                await _emailService.SendPasswordResetEmailAsync(request.Email, resetLink, actualName);

                return ApiResponse.SuccessResult("If an account with that email exists, a password reset link has been sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for {Email}", request.Email);
                return ApiResponse.ErrorResult("An error occurred while processing your request");
            }
        }

        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                var customUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.ResetToken == request.Token);
                if (customUser == null || customUser.ResetTokenExpiry == null || customUser.ResetTokenExpiry < DateTime.UtcNow)
                {
                    return ApiResponse.ErrorResult("Invalid or expired reset token");
                }

                var user = await _userManager.FindByEmailAsync(customUser.Email);
                if (user == null)
                {
                    return ApiResponse.ErrorResult("User not found");
                }

                // Reset password
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ApiResponse.ErrorResult("Password reset failed", errors);
                }

                // Clear reset token
                customUser.ResetToken = null;
                customUser.ResetTokenExpiry = null;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password reset successfully for user {Email}", customUser.Email);
                return ApiResponse.SuccessResult("Password reset successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return ApiResponse.ErrorResult("An error occurred while resetting password");
            }
        }

        private static string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}