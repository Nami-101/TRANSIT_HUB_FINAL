using Microsoft.AspNetCore.Mvc;
using TransitHub.Data;
using TransitHub.Models;

namespace TransitHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly TransitHubDbContext _context;

        public SeedController(TransitHubDbContext context)
        {
            _context = context;
        }

        [HttpPost("basic-data")]
        public async Task<IActionResult> SeedBasicData()
        {
            try
            {
                // Seed TrainQuotaTypes
                if (!_context.TrainQuotaTypes.Any())
                {
                    var quotaTypes = new[]
                    {
                        new TrainQuotaType { QuotaName = "Normal", Description = "Regular booking quota", IsActive = true },
                        new TrainQuotaType { QuotaName = "Tatkal", Description = "Emergency booking quota", IsActive = true },
                        new TrainQuotaType { QuotaName = "Senior Citizen", Description = "Senior citizen quota", IsActive = true }
                    };
                    await _context.TrainQuotaTypes.AddRangeAsync(quotaTypes);
                }

                // Seed TrainClasses
                if (!_context.TrainClasses.Any())
                {
                    var trainClasses = new[]
                    {
                        new TrainClass { ClassName = "SL", Description = "Sleeper Class", IsActive = true },
                        new TrainClass { ClassName = "3A", Description = "Third AC", IsActive = true },
                        new TrainClass { ClassName = "2A", Description = "Second AC", IsActive = true },
                        new TrainClass { ClassName = "1A", Description = "First AC", IsActive = true }
                    };
                    await _context.TrainClasses.AddRangeAsync(trainClasses);
                }

                // Seed Stations
                if (!_context.Stations.Any())
                {
                    var stations = new[]
                    {
                        new Station { StationName = "New Delhi Railway Station", City = "New Delhi", State = "Delhi", StationCode = "NDLS", IsActive = true },
                        new Station { StationName = "Mumbai Central", City = "Mumbai", State = "Maharashtra", StationCode = "BCT", IsActive = true },
                        new Station { StationName = "Howrah Junction", City = "Kolkata", State = "West Bengal", StationCode = "HWH", IsActive = true },
                        new Station { StationName = "Chennai Central", City = "Chennai", State = "Tamil Nadu", StationCode = "MAS", IsActive = true },
                        new Station { StationName = "Bangalore City Junction", City = "Bangalore", State = "Karnataka", StationCode = "SBC", IsActive = true }
                    };
                    await _context.Stations.AddRangeAsync(stations);
                }

                await _context.SaveChangesAsync();

                // Seed Trains (after stations are saved)
                if (!_context.Trains.Any())
                {
                    var trains = new[]
                    {
                        new Train { TrainName = "Rajdhani Express", TrainNumber = "12001", SourceStationID = 1, DestinationStationID = 2, IsActive = true },
                        new Train { TrainName = "Shatabdi Express", TrainNumber = "12002", SourceStationID = 1, DestinationStationID = 5, IsActive = true },
                        new Train { TrainName = "Duronto Express", TrainNumber = "12259", SourceStationID = 2, DestinationStationID = 3, IsActive = true }
                    };
                    await _context.Trains.AddRangeAsync(trains);
                }

                await _context.SaveChangesAsync();

                // Seed Train Schedules
                if (!_context.TrainSchedules.Any())
                {
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var schedules = new List<TrainSchedule>();

                    for (int i = 0; i < 7; i++) // Next 7 days
                    {
                        var travelDate = today.AddDays(i);
                        
                        schedules.AddRange(new[]
                        {
                            new TrainSchedule 
                            { 
                                TrainID = 1, TravelDate = travelDate, 
                                DepartureTime = DateTime.Today.AddHours(8), 
                                ArrivalTime = DateTime.Today.AddHours(20),
                                QuotaTypeID = 1, TrainClassID = 2, 
                                TotalSeats = 72, AvailableSeats = 45, Fare = 1500.00m,
                                IsActive = true
                            },
                            new TrainSchedule 
                            { 
                                TrainID = 2, TravelDate = travelDate, 
                                DepartureTime = DateTime.Today.AddHours(6), 
                                ArrivalTime = DateTime.Today.AddHours(14),
                                QuotaTypeID = 1, TrainClassID = 1, 
                                TotalSeats = 80, AvailableSeats = 52, Fare = 800.00m,
                                IsActive = true
                            }
                        });
                    }
                    
                    await _context.TrainSchedules.AddRangeAsync(schedules);
                }

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Basic data seeded successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message, StackTrace = ex.StackTrace });
            }
        }
    }
}