using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransitHub.Models.DTOs;
using TransitHub.Services.Interfaces;

namespace TransitHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard metrics
        /// </summary>
        [HttpGet("dashboard-metrics")]
        public async Task<ActionResult<ApiResponse<DashboardMetricsDto>>> GetDashboardMetrics()
        {
            try
            {
                var result = await _adminService.GetDashboardMetricsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard metrics");
                return StatusCode(500, ApiResponse<DashboardMetricsDto>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Get all users
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<ApiResponse<List<AdminUserDto>>>> GetAllUsers()
        {
            try
            {
                var result = await _adminService.GetAllUsersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, ApiResponse<List<AdminUserDto>>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Soft delete user
        /// </summary>
        [HttpDelete("users/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteUser(string id)
        {
            try
            {
                var result = await _adminService.SoftDeleteUserAsync(id);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, ApiResponse.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Get all trains
        /// </summary>
        [HttpGet("trains")]
        public async Task<ActionResult<ApiResponse<List<AdminTrainDto>>>> GetAllTrains()
        {
            try
            {
                var result = await _adminService.GetAllTrainsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trains");
                return StatusCode(500, ApiResponse<List<AdminTrainDto>>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Get train by ID
        /// </summary>
        [HttpGet("trains/{id}")]
        public async Task<ActionResult<ApiResponse<AdminTrainDto>>> GetTrainById(int id)
        {
            try
            {
                var result = await _adminService.GetTrainByIdAsync(id);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting train {TrainId}", id);
                return StatusCode(500, ApiResponse<AdminTrainDto>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Create new train
        /// </summary>
        [HttpPost("trains")]
        public async Task<ActionResult<ApiResponse<AdminTrainDto>>> CreateTrain([FromBody] CreateTrainDto createTrainDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _adminService.CreateTrainAsync(createTrainDto);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return CreatedAtAction(nameof(GetTrainById), new { id = result.Data?.TrainId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating train");
                return StatusCode(500, ApiResponse<AdminTrainDto>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Update train
        /// </summary>
        [HttpPut("trains/{id}")]
        public async Task<ActionResult<ApiResponse<AdminTrainDto>>> UpdateTrain(int id, [FromBody] UpdateTrainDto updateTrainDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _adminService.UpdateTrainAsync(id, updateTrainDto);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating train {TrainId}", id);
                return StatusCode(500, ApiResponse<AdminTrainDto>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Delete train
        /// </summary>
        [HttpDelete("trains/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteTrain(int id)
        {
            try
            {
                var result = await _adminService.DeleteTrainAsync(id);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting train {TrainId}", id);
                return StatusCode(500, ApiResponse.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Permanently remove train
        /// </summary>
        [HttpDelete("trains/{id}/remove")]
        public async Task<ActionResult<ApiResponse>> RemoveTrain(int id)
        {
            try
            {
                var result = await _adminService.RemoveTrainAsync(id);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing train {TrainId}", id);
                return StatusCode(500, ApiResponse.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Get all flights
        /// </summary>
        [HttpGet("flights")]
        public async Task<ActionResult<ApiResponse<List<AdminFlightDto>>>> GetAllFlights()
        {
            try
            {
                var result = await _adminService.GetAllFlightsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flights");
                return StatusCode(500, ApiResponse<List<AdminFlightDto>>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Get flight by ID
        /// </summary>
        [HttpGet("flights/{id}")]
        public async Task<ActionResult<ApiResponse<AdminFlightDto>>> GetFlightById(int id)
        {
            try
            {
                var result = await _adminService.GetFlightByIdAsync(id);
                if (!result.Success)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flight {FlightId}", id);
                return StatusCode(500, ApiResponse<AdminFlightDto>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Create new flight
        /// </summary>
        [HttpPost("flights")]
        public async Task<ActionResult<ApiResponse<AdminFlightDto>>> CreateFlight([FromBody] CreateFlightDto createFlightDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _adminService.CreateFlightAsync(createFlightDto);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return CreatedAtAction(nameof(GetFlightById), new { id = result.Data?.FlightId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating flight");
                return StatusCode(500, ApiResponse<AdminFlightDto>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Update flight
        /// </summary>
        [HttpPut("flights/{id}")]
        public async Task<ActionResult<ApiResponse<AdminFlightDto>>> UpdateFlight(int id, [FromBody] UpdateFlightDto updateFlightDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _adminService.UpdateFlightAsync(id, updateFlightDto);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating flight {FlightId}", id);
                return StatusCode(500, ApiResponse<AdminFlightDto>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Delete flight
        /// </summary>
        [HttpDelete("flights/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteFlight(int id)
        {
            try
            {
                var result = await _adminService.DeleteFlightAsync(id);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting flight {FlightId}", id);
                return StatusCode(500, ApiResponse.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Get booking reports
        /// </summary>
        [HttpGet("reports/bookings")]
        public async Task<ActionResult<ApiResponse<List<BookingReportDto>>>> GetBookingReports()
        {
            try
            {
                var result = await _adminService.GetBookingReportsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking reports");
                return StatusCode(500, ApiResponse<List<BookingReportDto>>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Get all stations for dropdowns
        /// </summary>
        [HttpGet("stations")]
        public async Task<ActionResult<ApiResponse<List<StationDto>>>> GetAllStations()
        {
            try
            {
                var result = await _adminService.GetAllStationsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stations");
                return StatusCode(500, ApiResponse<List<StationDto>>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Get all airports for dropdowns
        /// </summary>
        [HttpGet("airports")]
        public async Task<ActionResult<ApiResponse<List<AirportDto>>>> GetAllAirports()
        {
            try
            {
                var result = await _adminService.GetAllAirportsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting airports");
                return StatusCode(500, ApiResponse<List<AirportDto>>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Create new station
        /// </summary>
        [HttpPost("stations")]
        public async Task<ActionResult<ApiResponse<StationDto>>> CreateStation([FromBody] CreateStationDto createStationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _adminService.CreateStationAsync(createStationDto);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return CreatedAtAction(nameof(GetAllStations), result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating station");
                return StatusCode(500, ApiResponse<StationDto>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Update station
        /// </summary>
        [HttpPut("stations/{id}")]
        public async Task<ActionResult<ApiResponse<StationDto>>> UpdateStation(int id, [FromBody] UpdateStationDto updateStationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _adminService.UpdateStationAsync(id, updateStationDto);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating station {StationId}", id);
                return StatusCode(500, ApiResponse<StationDto>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Delete station
        /// </summary>
        [HttpDelete("stations/{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteStation(int id)
        {
            try
            {
                var result = await _adminService.DeleteStationAsync(id);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting station {StationId}", id);
                return StatusCode(500, ApiResponse.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Get train coaches
        /// </summary>
        [HttpGet("trains/{trainId}/coaches")]
        public async Task<ActionResult<ApiResponse<List<CoachDto>>>> GetTrainCoaches(int trainId)
        {
            try
            {
                var result = await _adminService.GetTrainCoachesAsync(trainId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coaches for train {TrainId}", trainId);
                return StatusCode(500, ApiResponse<List<CoachDto>>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Add coach to train
        /// </summary>
        [HttpPost("trains/{trainId}/coaches")]
        public async Task<ActionResult<ApiResponse<CoachDto>>> AddCoachToTrain(int trainId, [FromBody] CreateCoachDto createCoachDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _adminService.AddCoachToTrainAsync(trainId, createCoachDto);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return CreatedAtAction(nameof(GetTrainCoaches), new { trainId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding coach to train {TrainId}", trainId);
                return StatusCode(500, ApiResponse<CoachDto>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Update coach
        /// </summary>
        [HttpPut("coaches/{coachId}")]
        public async Task<ActionResult<ApiResponse<CoachDto>>> UpdateCoach(int coachId, [FromBody] UpdateCoachDto updateCoachDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _adminService.UpdateCoachAsync(coachId, updateCoachDto);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coach {CoachId}", coachId);
                return StatusCode(500, ApiResponse<CoachDto>.ErrorResult("Internal server error"));
            }
        }

        /// <summary>
        /// Delete coach
        /// </summary>
        [HttpDelete("coaches/{coachId}")]
        public async Task<ActionResult<ApiResponse>> DeleteCoach(int coachId)
        {
            try
            {
                var result = await _adminService.DeleteCoachAsync(coachId);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coach {CoachId}", coachId);
                return StatusCode(500, ApiResponse.ErrorResult("Internal server error"));
            }
        }
    }
}