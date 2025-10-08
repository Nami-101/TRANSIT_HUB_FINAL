using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitHub.Data;

namespace TransitHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResetController : ControllerBase
    {
        private readonly TransitHubDbContext _context;

        public ResetController(TransitHubDbContext context)
        {
            _context = context;
        }

        [HttpPost("availability")]
        public async Task<IActionResult> ResetAvailability()
        {
            try
            {
                // Reset train schedules
                await _context.Database.ExecuteSqlRawAsync("UPDATE TrainSchedules SET AvailableSeats = TotalSeats");
                
                // Reset seats
                await _context.Database.ExecuteSqlRawAsync("UPDATE Seats SET IsAvailable = 1");

                return Ok(new { Message = "Availability reset completed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}