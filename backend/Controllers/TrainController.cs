using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransitHub.Data;
using TransitHub.Models.DTOs;

namespace TransitHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainController : ControllerBase
    {
        private readonly TransitHubDbContext _context;
        private readonly ILogger<TrainController> _logger;

        public TrainController(TransitHubDbContext context, ILogger<TrainController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("stations")]
        public async Task<ActionResult<IEnumerable<StationDto>>> GetStations()
        {
            try
            {
                var stations = await _context.Stations
                    .Where(s => s.IsActive)
                    .Select(s => new StationDto
                    {
                        StationID = s.StationID,
                        StationName = s.StationName,
                        City = s.City,
                        State = s.State,
                        StationCode = s.StationCode
                    })
                    .OrderBy(s => s.StationName)
                    .ToListAsync();

                return Ok(stations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<TrainSearchResultDto>>> SearchTrains([FromBody] TrainSearchDto searchDto)
        {
            try
            {
                var query = from ts in _context.TrainSchedules
                           join t in _context.Trains on ts.TrainID equals t.TrainID
                           join ss in _context.Stations on t.SourceStationID equals ss.StationID
                           join ds in _context.Stations on t.DestinationStationID equals ds.StationID
                           join qt in _context.TrainQuotaTypes on ts.QuotaTypeID equals qt.QuotaTypeID
                           join tc in _context.TrainClasses on ts.TrainClassID equals tc.TrainClassID
                           where ts.IsActive && t.IsActive
                           select new
                           {
                               ts.ScheduleID,
                               ts.TrainID,
                               t.TrainName,
                               t.TrainNumber,
                               SourceStation = ss.StationName,
                               SourceStationCode = ss.StationCode,
                               SourceStationID = ss.StationID,
                               DestinationStation = ds.StationName,
                               DestinationStationCode = ds.StationCode,
                               DestinationStationID = ds.StationID,
                               ts.TravelDate,
                               ts.DepartureTime,
                               ts.ArrivalTime,
                               QuotaName = qt.QuotaName,
                               TrainClass = tc.ClassName,
                               ts.TotalSeats,
                               ts.AvailableSeats,
                               ts.Fare
                           };

                // Apply filters - use exact match for station names
                if (!string.IsNullOrEmpty(searchDto.SourceStation))
                {
                    query = query.Where(x => x.SourceStation == searchDto.SourceStation || x.SourceStationCode == searchDto.SourceStation);
                }

                if (!string.IsNullOrEmpty(searchDto.DestinationStation))
                {
                    query = query.Where(x => x.DestinationStation == searchDto.DestinationStation || x.DestinationStationCode == searchDto.DestinationStation);
                }

                query = query.Where(x => x.TravelDate == DateOnly.FromDateTime(searchDto.TravelDate));

                if (!string.IsNullOrEmpty(searchDto.TrainClass))
                {
                    query = query.Where(x => x.TrainClass == searchDto.TrainClass);
                }

                if (!string.IsNullOrEmpty(searchDto.Quota))
                {
                    query = query.Where(x => x.QuotaName == searchDto.Quota);
                }

                // Default sorting by departure time
                query = query.OrderBy(x => x.DepartureTime);

                var rawResults = await query.Select(x => new TrainSearchResultDto
                {
                    ScheduleID = x.ScheduleID,
                    TrainID = x.TrainID,
                    TrainName = x.TrainName,
                    TrainNumber = x.TrainNumber,
                    SourceStation = x.SourceStation,
                    SourceStationCode = x.SourceStationCode,
                    DestinationStation = x.DestinationStation,
                    DestinationStationCode = x.DestinationStationCode,
                    TravelDate = x.TravelDate,
                    DepartureTime = x.DepartureTime,
                    ArrivalTime = x.ArrivalTime,
                    JourneyTimeMinutes = EF.Functions.DateDiffMinute(x.DepartureTime, x.ArrivalTime),
                    QuotaName = x.QuotaName,
                    TrainClass = x.TrainClass,
                    TotalSeats = x.TotalSeats,
                    AvailableSeats = x.AvailableSeats,
                    Fare = x.Fare,
                    AvailabilityStatus = x.AvailableSeats > 0 ? "Available" : "Waitlist",
                    AvailableOrWaitlistPosition = x.AvailableSeats
                }).ToListAsync();

                // Group by train and class to eliminate quota duplicates
                var groupedResults = rawResults
                    .GroupBy(r => new { r.TrainID, r.TrainClass })
                    .Select(g => new {
                        ScheduleID = g.First().ScheduleID,
                        TrainID = g.Key.TrainID,
                        TrainName = g.First().TrainName,
                        TrainNumber = g.First().TrainNumber,
                        SourceStation = g.First().SourceStation,
                        SourceStationCode = g.First().SourceStationCode,
                        DestinationStation = g.First().DestinationStation,
                        DestinationStationCode = g.First().DestinationStationCode,
                        TravelDate = g.First().TravelDate,
                        DepartureTime = g.First().DepartureTime,
                        ArrivalTime = g.First().ArrivalTime,
                        JourneyTimeMinutes = g.First().JourneyTimeMinutes,
                        TrainClass = g.Key.TrainClass,
                        TotalSeats = g.Sum(x => x.TotalSeats),
                        AvailableSeats = g.Sum(x => x.AvailableSeats),
                        Fare = g.First().Fare
                    })
                    .ToList();

                var results = new List<TrainSearchResultDto>();
                
                foreach (var group in groupedResults)
                {
                    var availabilityStatus = group.AvailableSeats >= searchDto.PassengerCount ? "Available" : 
                                            group.AvailableSeats > 0 ? "Limited" : "Waitlist";
                    
                    var availableOrWaitlistPosition = availabilityStatus == "Waitlist" ? 
                        await GetWaitlistCountAsync(group.ScheduleID, group.TrainClass) : group.AvailableSeats;
                    
                    results.Add(new TrainSearchResultDto
                    {
                        ScheduleID = group.ScheduleID,
                        TrainID = group.TrainID,
                        TrainName = group.TrainName,
                        TrainNumber = group.TrainNumber,
                        SourceStation = group.SourceStation,
                        SourceStationCode = group.SourceStationCode,
                        DestinationStation = group.DestinationStation,
                        DestinationStationCode = group.DestinationStationCode,
                        TravelDate = group.TravelDate,
                        DepartureTime = group.DepartureTime,
                        ArrivalTime = group.ArrivalTime,
                        JourneyTimeMinutes = group.JourneyTimeMinutes,
                        QuotaName = "General",
                        TrainClass = group.TrainClass,
                        TotalSeats = group.TotalSeats,
                        AvailableSeats = group.AvailableSeats,
                        Fare = group.Fare,
                        AvailabilityStatus = availabilityStatus,
                        AvailableOrWaitlistPosition = availableOrWaitlistPosition
                    });
                }
                
                results = results.OrderBy(r => r.DepartureTime).ThenBy(r => r.TrainClass).ToList();

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("classes")]
        public async Task<ActionResult<IEnumerable<TrainClassDto>>> GetTrainClasses()
        {
            try
            {
                var classes = await _context.TrainClasses
                    .Where(tc => tc.IsActive)
                    .Select(tc => new TrainClassDto
                    {
                        TrainClassID = tc.TrainClassID,
                        ClassName = tc.ClassName,
                        Description = tc.Description
                    })
                    .ToListAsync();

                return Ok(classes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("quotas")]
        public async Task<ActionResult<IEnumerable<TrainQuotaTypeDto>>> GetQuotaTypes()
        {
            try
            {
                var quotas = await _context.TrainQuotaTypes
                    .Where(qt => qt.IsActive)
                    .Select(qt => new TrainQuotaTypeDto
                    {
                        QuotaTypeID = qt.QuotaTypeID,
                        QuotaName = qt.QuotaName,
                        Description = qt.Description
                    })
                    .ToListAsync();

                return Ok(quotas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("debug/trains")]
        public async Task<ActionResult> GetDebugTrains()
        {
            try
            {
                var trains = await _context.Trains
                    .Include(t => t.SourceStation)
                    .Include(t => t.DestinationStation)
                    .Select(t => new {
                        t.TrainID,
                        t.TrainName,
                        t.TrainNumber,
                        SourceStation = t.SourceStation.StationName,
                        DestinationStation = t.DestinationStation.StationName
                    })
                    .ToListAsync();

                return Ok(trains);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("debug/schedules")]
        public async Task<ActionResult> GetDebugSchedules()
        {
            try
            {
                var schedules = await _context.TrainSchedules
                    .Include(ts => ts.Train)
                    .ThenInclude(t => t.SourceStation)
                    .Include(ts => ts.Train)
                    .ThenInclude(t => t.DestinationStation)
                    .Select(ts => new {
                        ts.ScheduleID,
                        ts.TrainID,
                        TrainName = ts.Train.TrainName,
                        SourceStation = ts.Train.SourceStation.StationName,
                        DestinationStation = ts.Train.DestinationStation.StationName,
                        ts.TravelDate,
                        ts.IsActive
                    })
                    .ToListAsync();

                return Ok(schedules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPost("cleanup-seats")]
        public async Task<ActionResult> CleanupCancelledBookingSeats()
        {
            try
            {
                // Clear seat numbers from cancelled bookings
                var cancelledPassengers = await _context.BookingPassengers
                    .Where(p => p.Booking.StatusID == 3 && !string.IsNullOrEmpty(p.SeatNumber))
                    .ToListAsync();
                
                foreach (var passenger in cancelledPassengers)
                {
                    passenger.SeatNumber = null;
                }
                
                await _context.SaveChangesAsync();
                
                return Ok(new { Message = $"Cleared seat numbers from {cancelledPassengers.Count} cancelled passengers" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        private async Task<int> GetWaitlistCountAsync(int scheduleId, string trainClass)
        {
            try
            {
                var count = await _context.WaitlistQueues
                    .Where(w => w.TrainScheduleID == scheduleId && w.TrainClass == trainClass && w.IsActive)
                    .CountAsync();
                
                _logger.LogInformation("Waitlist count for schedule {ScheduleId} class {TrainClass}: {Count}", scheduleId, trainClass, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting waitlist count for schedule {ScheduleId} class {TrainClass}, defaulting to 0", scheduleId, trainClass);
                return 0;
            }
        }
    }
}