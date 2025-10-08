using TransitHub.Models.DTOs;

namespace TransitHub.Services.Interfaces
{
    public interface ITrainBookingService
    {
        Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request);
        Task<CancelBookingResponse> CancelBookingAsync(CancelBookingRequest request);
        Task<BookingDetails> GetBookingDetailsAsync(int bookingId);
        Task<List<BookingDetails>> GetUserBookingsAsync();
        Task<CoachLayoutResponse> GetCoachLayoutAsync(int scheduleId, string trainClass);
        Task<WaitlistInfo> GetWaitlistInfoAsync(int scheduleId);
        Task<AddCoachResponse> AddCoachesAsync(int trainId, AddCoachRequest request);
        Task RemoveBookingAsync(int bookingId);
        Task RemoveMultipleBookingsAsync(List<int> bookingIds);
    }
}