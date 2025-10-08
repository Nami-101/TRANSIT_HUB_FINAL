using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransitHub.Models.DTOs;
using TransitHub.Services.Interfaces;

namespace TransitHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainBookingController : ControllerBase
    {
        private readonly ITrainBookingService _trainBookingService;
        private readonly ILogger<TrainBookingController> _logger;

        public TrainBookingController(ITrainBookingService trainBookingService, ILogger<TrainBookingController> logger)
        {
            _trainBookingService = trainBookingService;
            _logger = logger;
        }

        [HttpPost("book")]
        [Authorize]
        public async Task<ActionResult<BookingResponse>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                var result = await _trainBookingService.CreateBookingAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, new BookingResponse 
                { 
                    Success = false, 
                    Message = "An error occurred while processing your booking" 
                });
            }
        }

        [HttpPost("cancel")]
        [Authorize]
        public async Task<ActionResult<CancelBookingResponse>> CancelBooking([FromBody] CancelBookingRequest request)
        {
            try
            {
                var result = await _trainBookingService.CancelBookingAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                return StatusCode(500, new CancelBookingResponse 
                { 
                    Success = false, 
                    Message = "An error occurred while cancelling your booking" 
                });
            }
        }

        [HttpGet("coach-layout/{trainId}/{travelDate}")]
        public async Task<ActionResult<CoachLayoutResponse>> GetCoachLayout(int trainId, string travelDate)
        {
            try
            {
                var result = await _trainBookingService.GetCoachLayoutAsync(trainId, travelDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coach layout");
                return StatusCode(500, new CoachLayoutResponse());
            }
        }

        [HttpPost("add-coaches/{trainId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AddCoachResponse>> AddCoaches(int trainId, [FromBody] AddCoachRequest request)
        {
            try
            {
                var result = await _trainBookingService.AddCoachesAsync(trainId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding coaches");
                return StatusCode(500, new AddCoachResponse 
                { 
                    Success = false, 
                    Message = "An error occurred while adding coaches" 
                });
            }
        }

        [HttpGet("waitlist/{scheduleId}")]
        public async Task<ActionResult<WaitlistInfo>> GetWaitlistInfo(int scheduleId)
        {
            try
            {
                var result = await _trainBookingService.GetWaitlistInfoAsync(scheduleId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting waitlist info");
                return StatusCode(500, new WaitlistInfo());
            }
        }

        [HttpGet("{bookingId}")]
        [Authorize]
        public async Task<ActionResult<BookingDetails>> GetBookingDetails(int bookingId)
        {
            try
            {
                var result = await _trainBookingService.GetBookingDetailsAsync(bookingId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking details");
                return NotFound();
            }
        }

        [HttpGet("my-bookings")]
        [Authorize]
        public async Task<ActionResult<List<BookingDetails>>> GetMyBookings()
        {
            try
            {
                var result = await _trainBookingService.GetUserBookingsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookings");
                return StatusCode(500, new List<BookingDetails>());
            }
        }

        [HttpDelete("{bookingId}")]
        [Authorize]
        public async Task<ActionResult> RemoveBooking(int bookingId)
        {
            try
            {
                await _trainBookingService.RemoveBookingAsync(bookingId);
                return Ok(new { success = true, message = "Booking removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing booking {BookingId}", bookingId);
                return StatusCode(500, new { success = false, message = "Failed to remove booking" });
            }
        }

        [HttpPost("remove-multiple")]
        [Authorize]
        public async Task<ActionResult> RemoveMultipleBookings([FromBody] RemoveMultipleBookingsRequest request)
        {
            try
            {
                await _trainBookingService.RemoveMultipleBookingsAsync(request.BookingIds);
                return Ok(new { success = true, message = $"{request.BookingIds.Count} bookings removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing multiple bookings");
                return StatusCode(500, new { success = false, message = "Failed to remove bookings" });
            }
        }
    }

    public class RemoveMultipleBookingsRequest
    {
        public List<int> BookingIds { get; set; } = new List<int>();
    }
}