using TransitHub.Models.DTOs;

namespace TransitHub.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<LoginResponse>> LoginWithRoleAsync(LoginWithRoleRequest request);
        Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken);
        Task<ApiResponse> LogoutAsync(string userId);
        Task<ApiResponse> RegisterAsync(RegisterRequest request);
        Task<ApiResponse> VerifyEmailAsync(VerifyEmailRequest request);
        Task<ApiResponse> ResendOtpAsync(ResendOtpRequest request);
        Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string frontendUrl = "http://localhost:4200");
        Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request);
    }
}