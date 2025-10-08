using TransitHub.Models.DTOs;

namespace TransitHub.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(string userId);
        Task<ApiResponse> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto);
        Task<ApiResponse<List<UserBookingDto>>> GetUserBookingsAsync(string userId);
        Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
    }
}