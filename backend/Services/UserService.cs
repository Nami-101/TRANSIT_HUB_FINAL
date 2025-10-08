using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransitHub.Data;
using TransitHub.Models;
using TransitHub.Models.DTOs;
using TransitHub.Repositories.Interfaces;
using TransitHub.Services.Interfaces;

namespace TransitHub.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TransitHubDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(
            UserManager<IdentityUser> userManager,
            IUnitOfWork unitOfWork,
            TransitHubDbContext context,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserProfileDto>.ErrorResult("User not found");
                }

                var userProfile = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.UserName!, // Adjust based on your user model
                    LastName = "",
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed
                };

                return ApiResponse<UserProfileDto>.SuccessResult(userProfile, "User profile retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile for user {UserId}", userId);
                return ApiResponse<UserProfileDto>.ErrorResult("Failed to retrieve user profile");
            }
        }

        public async Task<ApiResponse> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse.ErrorResult("User not found");
                }

                // Update user properties
                user.PhoneNumber = updateDto.PhoneNumber;
                // Add other properties as needed

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User profile updated successfully for user {UserId}", userId);
                    return ApiResponse.SuccessResult("Profile updated successfully");
                }

                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.ErrorResult("Failed to update profile", errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile for user {UserId}", userId);
                return ApiResponse.ErrorResult("Failed to update user profile");
            }
        }

        public async Task<ApiResponse<List<UserBookingDto>>> GetUserBookingsAsync(string userId)
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.Passengers)
                    .Include(b => b.Status)
                    .Include(b => b.Payments)
                    .Where(b => b.UserID == int.Parse(userId) && b.IsActive)
                    .OrderByDescending(b => b.BookingDate)
                    .Select(b => new UserBookingDto
                    {
                        BookingID = b.BookingID,
                        BookingReference = b.BookingReference,
                        BookingType = b.BookingType,
                        Status = b.Status.StatusName,
                        TotalAmount = b.TotalAmount,
                        BookingDate = b.BookingDate,
                        TotalPassengers = b.TotalPassengers
                    })
                    .ToListAsync();

                return ApiResponse<List<UserBookingDto>>.SuccessResult(bookings, "User bookings retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for user {UserId}", userId);
                return ApiResponse<List<UserBookingDto>>.ErrorResult("Failed to retrieve user bookings");
            }
        }

        public async Task<ApiResponse> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse.ErrorResult("User not found");
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                    return ApiResponse.SuccessResult("Password changed successfully");
                }

                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse.ErrorResult("Failed to change password", errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return ApiResponse.ErrorResult("Failed to change password");
            }
        }
    }
}