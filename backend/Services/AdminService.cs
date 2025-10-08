using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransitHub.Data;
using TransitHub.Models;
using TransitHub.Models.DTOs;
using TransitHub.Repositories.Interfaces;
using TransitHub.Services.Interfaces;

namespace TransitHub.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TransitHubDbContext _context;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            TransitHubDbContext context,
            ILogger<AdminService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<DashboardMetricsDto>> GetDashboardMetricsAsync()
        {
            try
            {
                var metrics = new DashboardMetricsDto
                {
                    TotalUsers = await _context.Users.CountAsync(),
                    TotalTrains = await _context.Trains.CountAsync(t => t.IsActive),
                    TotalFlights = await _context.Flights.CountAsync(f => f.IsActive),
                    TotalBookings = await _context.Bookings.CountAsync(b => b.IsActive),
                    TotalStations = await _context.Stations.CountAsync(s => s.IsActive),
                    TotalAirports = await _context.Airports.CountAsync(a => a.IsActive),
                    TotalRevenue = await _context.Bookings
                        .Where(b => b.IsActive)
                        .SumAsync(b => b.TotalAmount),
                    ActiveBookings = await _context.Bookings
                        .CountAsync(b => b.StatusID == 1 && b.IsActive) // Confirmed and Active
                };

                return ApiResponse<DashboardMetricsDto>.SuccessResult(metrics, "Dashboard metrics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard metrics");
                return ApiResponse<DashboardMetricsDto>.ErrorResult("Failed to retrieve dashboard metrics");
            }
        }

        public async Task<ApiResponse<List<AdminUserDto>>> GetAllUsersAsync()
        {
            try
            {
                var identityUsers = await _userManager.Users.ToListAsync();
                var userDtos = new List<AdminUserDto>();

                foreach (var user in identityUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var customUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                    
                    userDtos.Add(new AdminUserDto
                    {
                        Id = user.Id,
                        Name = customUser?.Name ?? user.Email ?? "",
                        Email = user.Email ?? "",
                        Roles = roles.ToList(),
                        EmailConfirmed = user.EmailConfirmed,
                        CreatedAt = DateTime.UtcNow, // Identity doesn't have CreatedAt by default
                        IsActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd < DateTime.UtcNow
                    });
                }

                return ApiResponse<List<AdminUserDto>>.SuccessResult(userDtos, "Users retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return ApiResponse<List<AdminUserDto>>.ErrorResult("Failed to retrieve users");
            }
        }

        public async Task<ApiResponse> SoftDeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse.ErrorResult("User not found");
                }

                // Soft delete by setting lockout end to far future
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} soft deleted successfully", userId);
                    return ApiResponse.SuccessResult("User deactivated successfully");
                }

                return ApiResponse.ErrorResult("Failed to deactivate user");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting user {UserId}", userId);
                return ApiResponse.ErrorResult("Failed to deactivate user");
            }
        }

        public async Task<ApiResponse<List<AdminTrainDto>>> GetAllTrainsAsync()
        {
            try
            {
                var trains = await _context.Trains
                    .Include(t => t.SourceStation)
                    .Include(t => t.DestinationStation)
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => new AdminTrainDto
                    {
                        TrainId = t.TrainID,
                        TrainNumber = t.TrainNumber,
                        TrainName = t.TrainName,
                        SourceStationId = t.SourceStationID,
                        SourceStationName = t.SourceStation.StationName,
                        DestinationStationId = t.DestinationStationID,
                        DestinationStationName = t.DestinationStation.StationName,
                        DepartureTime = "00:00", // Default - schedules manage actual times
                        ArrivalTime = "00:00", // Default - schedules manage actual times
                        BasePrice = 0, // Default - schedules manage actual pricing
                        IsActive = t.IsActive,
                        CreatedAt = t.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<AdminTrainDto>>.SuccessResult(trains, "Trains retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trains");
                return ApiResponse<List<AdminTrainDto>>.ErrorResult("Failed to retrieve trains");
            }
        }

        public async Task<ApiResponse<AdminTrainDto>> GetTrainByIdAsync(int trainId)
        {
            try
            {
                var train = await _context.Trains
                    .Include(t => t.SourceStation)
                    .Include(t => t.DestinationStation)
                    .Where(t => t.TrainID == trainId)
                    .Select(t => new AdminTrainDto
                    {
                        TrainId = t.TrainID,
                        TrainNumber = t.TrainNumber,
                        TrainName = t.TrainName,
                        SourceStationId = t.SourceStationID,
                        SourceStationName = t.SourceStation.StationName,
                        DestinationStationId = t.DestinationStationID,
                        DestinationStationName = t.DestinationStation.StationName,
                        DepartureTime = "00:00", // Default - schedules manage actual times
                        ArrivalTime = "00:00", // Default - schedules manage actual times
                        BasePrice = 0, // Default - schedules manage actual pricing
                        IsActive = t.IsActive,
                        CreatedAt = t.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (train == null)
                {
                    return ApiResponse<AdminTrainDto>.ErrorResult("Train not found");
                }

                return ApiResponse<AdminTrainDto>.SuccessResult(train, "Train retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving train {TrainId}", trainId);
                return ApiResponse<AdminTrainDto>.ErrorResult("Failed to retrieve train");
            }
        }

        public async Task<ApiResponse<AdminTrainDto>> CreateTrainAsync(CreateTrainDto createTrainDto)
        {
            try
            {
                // Check if train number already exists
                var existingTrain = await _context.Trains
                    .FirstOrDefaultAsync(t => t.TrainNumber == createTrainDto.TrainNumber);
                
                if (existingTrain != null)
                {
                    return ApiResponse<AdminTrainDto>.ErrorResult(
                        $"🚂 Oops! Train number {createTrainDto.TrainNumber} is already taken. Please choose a different number.");
                }

                var train = new Train
                {
                    TrainNumber = createTrainDto.TrainNumber,
                    TrainName = createTrainDto.TrainName,
                    SourceStationID = createTrainDto.SourceStationId,
                    DestinationStationID = createTrainDto.DestinationStationId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                };

                _context.Trains.Add(train);
                await _context.SaveChangesAsync();

                // Create train coaches only for selected classes
                await CreateTrainCoachesAsync(train.TrainID, createTrainDto.DepartureTime, 
                    createTrainDto.ArrivalTime, createTrainDto.BasePrice, createTrainDto.SelectedClassIds);

                var trainDto = await GetTrainByIdAsync(train.TrainID);
                return trainDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating train");
                return ApiResponse<AdminTrainDto>.ErrorResult("Failed to create train");
            }
        }

        private async Task CreateTrainCoachesAsync(int trainId, string departureTime, string arrivalTime, decimal basePrice, List<int> selectedClassIds)
        {
            try
            {
                // Get only selected train classes and first quota type (General)
                var trainClasses = await _context.TrainClasses
                    .Where(tc => selectedClassIds.Contains(tc.TrainClassID))
                    .ToListAsync();
                var generalQuota = await _context.TrainQuotaTypes.FirstAsync(qt => qt.QuotaName == "Normal");

                // Parse times
                var depTime = TimeOnly.Parse(departureTime);
                var arrTime = TimeOnly.Parse(arrivalTime);

                var schedules = new List<TrainSchedule>();

                // Create schedules for the next 30 days
                for (int day = 0; day < 30; day++)
                {
                    var travelDate = DateOnly.FromDateTime(DateTime.Today.AddDays(day));
                    var departureDateTime = travelDate.ToDateTime(depTime);
                    var arrivalDateTime = travelDate.ToDateTime(arrTime);

                    // If arrival is before departure, it means next day arrival
                    if (arrTime < depTime)
                    {
                        arrivalDateTime = arrivalDateTime.AddDays(1);
                    }

                    foreach (var trainClass in trainClasses)
                    {
                        // Calculate seats based on IRCTC class standards
                        int totalSeats = trainClass.TrainClassID switch
                        {
                            1 => 72,  // Sleeper Class (SL)
                            2 => 78,  // AC 3 Tier (3A)
                            3 => 64,  // AC 2 Tier (2A)
                            4 => 18,  // AC 1st Class (1A)
                            5 => 72,  // Chair Car (CC)
                            6 => 72,  // Second Sitting (2S)
                            7 => 16,  // First Class (FC)
                            _ => 72
                        };

                        decimal classFare = trainClass.TrainClassID switch
                        {
                            1 => basePrice,           // Sleeper - base price
                            2 => basePrice * 1.5m,   // AC 3 Tier - 50% more
                            3 => basePrice * 2.0m,   // AC 2 Tier - 100% more
                            4 => basePrice * 3.0m,   // AC 1st Class - 200% more
                            _ => basePrice
                        };

                        // Create only ONE schedule entry per class (not per quota)
                        var schedule = new TrainSchedule
                        {
                            TrainID = trainId,
                            TravelDate = travelDate,
                            DepartureTime = departureDateTime,
                            ArrivalTime = arrivalDateTime,
                            QuotaTypeID = generalQuota.QuotaTypeID,
                            TrainClassID = trainClass.TrainClassID,
                            TotalSeats = totalSeats,
                            AvailableSeats = totalSeats,
                            Fare = classFare,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "Admin"
                        };

                        schedules.Add(schedule);
                    }
                }

                _context.TrainSchedules.AddRange(schedules);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created {Count} train schedules for train {TrainId}", schedules.Count, trainId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating train schedules for train {TrainId}", trainId);
                throw;
            }
        }

        public async Task<ApiResponse<AdminTrainDto>> UpdateTrainAsync(int trainId, UpdateTrainDto updateTrainDto)
        {
            try
            {
                var train = await _context.Trains.FindAsync(trainId);
                if (train == null)
                {
                    return ApiResponse<AdminTrainDto>.ErrorResult("Train not found");
                }

                train.TrainName = updateTrainDto.TrainName;
                train.SourceStationID = updateTrainDto.SourceStationId;
                train.DestinationStationID = updateTrainDto.DestinationStationId;
                train.IsActive = updateTrainDto.IsActive;
                train.UpdatedAt = DateTime.UtcNow;
                train.UpdatedBy = "Admin";

                await _context.SaveChangesAsync();

                var trainDto = await GetTrainByIdAsync(trainId);
                return trainDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating train {TrainId}", trainId);
                return ApiResponse<AdminTrainDto>.ErrorResult("Failed to update train");
            }
        }

        public async Task<ApiResponse> DeleteTrainAsync(int trainId)
        {
            try
            {
                var train = await _context.Trains.FindAsync(trainId);
                if (train == null)
                {
                    return ApiResponse.ErrorResult("Train not found");
                }

                train.IsActive = false;
                train.UpdatedAt = DateTime.UtcNow;
                train.UpdatedBy = "Admin";

                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult("Train deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting train {TrainId}", trainId);
                return ApiResponse.ErrorResult("Failed to delete train");
            }
        }

        public async Task<ApiResponse> RemoveTrainAsync(int trainId)
        {
            try
            {
                var train = await _context.Trains.FindAsync(trainId);
                if (train == null)
                {
                    return ApiResponse.ErrorResult("Train not found");
                }

                if (train.IsActive)
                {
                    return ApiResponse.ErrorResult("Cannot permanently remove an active train. Please delete it first.");
                }

                // Get schedule IDs for this train
                var scheduleIds = await _context.TrainSchedules
                    .Where(ts => ts.TrainID == trainId)
                    .Select(ts => ts.ScheduleID)
                    .ToListAsync();

                // Remove all related data first
                var bookings = await _context.Bookings
                    .Where(b => b.TrainScheduleID.HasValue && scheduleIds.Contains(b.TrainScheduleID.Value))
                    .ToListAsync();
                
                if (bookings.Any())
                {
                    var bookingIds = bookings.Select(b => b.BookingID).ToList();
                    
                    // Remove booking passengers
                    var passengers = await _context.BookingPassengers
                        .Where(p => bookingIds.Contains(p.BookingID))
                        .ToListAsync();
                    _context.BookingPassengers.RemoveRange(passengers);
                    
                    // Remove payments
                    var payments = await _context.Payments
                        .Where(p => bookingIds.Contains(p.BookingID))
                        .ToListAsync();
                    _context.Payments.RemoveRange(payments);
                    
                    // Remove bookings
                    _context.Bookings.RemoveRange(bookings);
                }

                // Remove schedules
                var schedules = await _context.TrainSchedules.Where(ts => ts.TrainID == trainId).ToListAsync();
                _context.TrainSchedules.RemoveRange(schedules);
                
                // Remove the train
                _context.Trains.Remove(train);

                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult("Train permanently removed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing train {TrainId}", trainId);
                return ApiResponse.ErrorResult("Failed to remove train");
            }
        }

        public async Task<ApiResponse<List<AdminFlightDto>>> GetAllFlightsAsync()
        {
            try
            {
                var flights = await _context.Flights
                    .Include(f => f.SourceAirport)
                    .Include(f => f.DestinationAirport)
                    .Select(f => new AdminFlightDto
                    {
                        FlightId = f.FlightID,
                        FlightNumber = f.FlightNumber,
                        Airline = f.Airline,
                        SourceAirportId = f.SourceAirportID,
                        SourceAirportName = f.SourceAirport.AirportName,
                        DestinationAirportId = f.DestinationAirportID,
                        DestinationAirportName = f.DestinationAirport.AirportName,
                        DepartureTime = "00:00", // Default - schedules manage actual times
                        ArrivalTime = "00:00", // Default - schedules manage actual times
                        BasePrice = 0, // Default - schedules manage actual pricing
                        IsActive = f.IsActive,
                        CreatedAt = f.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<AdminFlightDto>>.SuccessResult(flights, "Flights retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flights");
                return ApiResponse<List<AdminFlightDto>>.ErrorResult("Failed to retrieve flights");
            }
        }

        public async Task<ApiResponse<AdminFlightDto>> GetFlightByIdAsync(int flightId)
        {
            try
            {
                var flight = await _context.Flights
                    .Include(f => f.SourceAirport)
                    .Include(f => f.DestinationAirport)
                    .Where(f => f.FlightID == flightId)
                    .Select(f => new AdminFlightDto
                    {
                        FlightId = f.FlightID,
                        FlightNumber = f.FlightNumber,
                        Airline = f.Airline,
                        SourceAirportId = f.SourceAirportID,
                        SourceAirportName = f.SourceAirport.AirportName,
                        DestinationAirportId = f.DestinationAirportID,
                        DestinationAirportName = f.DestinationAirport.AirportName,
                        DepartureTime = "00:00", // Default - schedules manage actual times
                        ArrivalTime = "00:00", // Default - schedules manage actual times
                        BasePrice = 0, // Default - schedules manage actual pricing
                        IsActive = f.IsActive,
                        CreatedAt = f.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (flight == null)
                {
                    return ApiResponse<AdminFlightDto>.ErrorResult("Flight not found");
                }

                return ApiResponse<AdminFlightDto>.SuccessResult(flight, "Flight retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving flight {FlightId}", flightId);
                return ApiResponse<AdminFlightDto>.ErrorResult("Failed to retrieve flight");
            }
        }

        public async Task<ApiResponse<AdminFlightDto>> CreateFlightAsync(CreateFlightDto createFlightDto)
        {
            try
            {
                var flight = new Flight
                {
                    FlightNumber = createFlightDto.FlightNumber,
                    Airline = createFlightDto.Airline,
                    SourceAirportID = createFlightDto.SourceAirportId,
                    DestinationAirportID = createFlightDto.DestinationAirportId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                };

                _context.Flights.Add(flight);
                await _context.SaveChangesAsync();

                // Create default flight schedules for the next 30 days
                await CreateDefaultFlightSchedulesAsync(flight.FlightID, createFlightDto.DepartureTime, createFlightDto.ArrivalTime, createFlightDto.BasePrice);

                var flightDto = await GetFlightByIdAsync(flight.FlightID);
                return flightDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating flight");
                return ApiResponse<AdminFlightDto>.ErrorResult("Failed to create flight");
            }
        }

        private async Task CreateDefaultFlightSchedulesAsync(int flightId, string departureTime, string arrivalTime, decimal basePrice)
        {
            try
            {
                // Get all flight classes
                var flightClasses = await _context.FlightClasses.ToListAsync();

                // Parse times
                var depTime = TimeOnly.Parse(departureTime);
                var arrTime = TimeOnly.Parse(arrivalTime);

                var schedules = new List<FlightSchedule>();

                // Create schedules for the next 30 days
                for (int day = 0; day < 30; day++)
                {
                    var travelDate = DateOnly.FromDateTime(DateTime.Today.AddDays(day));
                    var departureDateTime = travelDate.ToDateTime(depTime);
                    var arrivalDateTime = travelDate.ToDateTime(arrTime);

                    // If arrival is before departure, it means next day arrival
                    if (arrTime < depTime)
                    {
                        arrivalDateTime = arrivalDateTime.AddDays(1);
                    }

                    foreach (var flightClass in flightClasses)
                    {
                        // Calculate seats and fare based on class
                        int totalSeats = flightClass.FlightClassID switch
                        {
                            1 => 180, // Economy
                            2 => 24,  // Business
                            3 => 12,  // First Class
                            _ => 180
                        };

                        decimal classFare = flightClass.FlightClassID switch
                        {
                            1 => basePrice,           // Economy - base price
                            2 => basePrice * 2.5m,   // Business - 150% more
                            3 => basePrice * 4.0m,   // First Class - 300% more
                            _ => basePrice
                        };

                        var schedule = new FlightSchedule
                        {
                            FlightID = flightId,
                            TravelDate = travelDate,
                            DepartureTime = departureDateTime,
                            ArrivalTime = arrivalDateTime,
                            FlightClassID = flightClass.FlightClassID,
                            TotalSeats = totalSeats,
                            AvailableSeats = totalSeats,
                            Fare = classFare,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "Admin"
                        };

                        schedules.Add(schedule);
                    }
                }

                _context.FlightSchedules.AddRange(schedules);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created {Count} flight schedules for flight {FlightId}", schedules.Count, flightId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating default flight schedules for flight {FlightId}", flightId);
                throw;
            }
        }

        public async Task<ApiResponse<AdminFlightDto>> UpdateFlightAsync(int flightId, UpdateFlightDto updateFlightDto)
        {
            try
            {
                var flight = await _context.Flights.FindAsync(flightId);
                if (flight == null)
                {
                    return ApiResponse<AdminFlightDto>.ErrorResult("Flight not found");
                }

                flight.Airline = updateFlightDto.Airline;
                flight.SourceAirportID = updateFlightDto.SourceAirportId;
                flight.DestinationAirportID = updateFlightDto.DestinationAirportId;
                flight.IsActive = updateFlightDto.IsActive;
                flight.UpdatedAt = DateTime.UtcNow;
                flight.UpdatedBy = "Admin";

                await _context.SaveChangesAsync();

                var flightDto = await GetFlightByIdAsync(flightId);
                return flightDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating flight {FlightId}", flightId);
                return ApiResponse<AdminFlightDto>.ErrorResult("Failed to update flight");
            }
        }

        public async Task<ApiResponse> DeleteFlightAsync(int flightId)
        {
            try
            {
                var flight = await _context.Flights.FindAsync(flightId);
                if (flight == null)
                {
                    return ApiResponse.ErrorResult("Flight not found");
                }

                flight.IsActive = false;
                flight.UpdatedAt = DateTime.UtcNow;
                flight.UpdatedBy = "Admin";

                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult("Flight deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting flight {FlightId}", flightId);
                return ApiResponse.ErrorResult("Failed to delete flight");
            }
        }

        public async Task<ApiResponse<List<BookingReportDto>>> GetBookingReportsAsync()
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.User)
                    .Where(b => b.IsActive)
                    .ToListAsync();

                var bookingReports = new List<BookingReportDto>();

                foreach (var booking in bookings)
                {
                    var report = new BookingReportDto
                    {
                        BookingId = booking.BookingID,
                        BookingReference = booking.BookingReference,
                        UserName = booking.User?.Name ?? "Unknown",
                        UserEmail = booking.User?.Email ?? "Unknown",
                        TransportType = booking.BookingType,
                        TransportNumber = "N/A",
                        Route = "N/A",
                        BookingDate = booking.BookingDate,
                        TravelDate = booking.BookingDate,
                        Amount = booking.TotalAmount,
                        Status = GetBookingStatusName(booking.StatusID),
                        PaymentStatus = "Paid"
                    };

                    // Get train details if it's a train booking
                    if (booking.TrainScheduleID.HasValue)
                    {
                        var schedule = await _context.TrainSchedules
                            .Include(ts => ts.Train)
                                .ThenInclude(t => t.SourceStation)
                            .Include(ts => ts.Train)
                                .ThenInclude(t => t.DestinationStation)
                            .FirstOrDefaultAsync(ts => ts.ScheduleID == booking.TrainScheduleID.Value);

                        if (schedule != null)
                        {
                            report.TransportNumber = schedule.Train?.TrainNumber ?? "N/A";
                            report.Route = schedule.Train?.SourceStation != null && schedule.Train?.DestinationStation != null
                                ? $"{schedule.Train.SourceStation.StationName} → {schedule.Train.DestinationStation.StationName}"
                                : "N/A";
                            report.TravelDate = schedule.TravelDate.ToDateTime(TimeOnly.MinValue);
                        }
                    }

                    bookingReports.Add(report);
                }

                var sortedReports = bookingReports.OrderByDescending(b => b.BookingDate).ToList();
                return ApiResponse<List<BookingReportDto>>.SuccessResult(sortedReports, "Booking reports retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking reports: {Error}", ex.Message);
                return ApiResponse<List<BookingReportDto>>.ErrorResult("Failed to retrieve booking reports");
            }
        }

        private string GetBookingStatusName(int statusId)
        {
            return statusId switch
            {
                1 => "Confirmed",
                2 => "Waitlisted",
                3 => "Cancelled",
                _ => "Unknown"
            };
        }

        public async Task<ApiResponse<List<StationDto>>> GetAllStationsAsync()
        {
            try
            {
                var stations = await _context.Stations
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => new StationDto
                    {
                        StationID = s.StationID,
                        StationName = s.StationName,
                        City = s.City,
                        State = s.State,
                        StationCode = s.StationCode
                    })
                    .ToListAsync();

                return ApiResponse<List<StationDto>>.SuccessResult(stations, "Stations retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stations");
                return ApiResponse<List<StationDto>>.ErrorResult("Failed to retrieve stations");
            }
        }

        public async Task<ApiResponse<List<AirportDto>>> GetAllAirportsAsync()
        {
            try
            {
                var airports = await _context.Airports
                    .Where(a => a.IsActive)
                    .Select(a => new AirportDto
                    {
                        AirportID = a.AirportID,
                        AirportName = a.AirportName,
                        City = a.City,
                        State = a.State,
                        Code = a.Code
                    })
                    .ToListAsync();

                return ApiResponse<List<AirportDto>>.SuccessResult(airports, "Airports retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving airports");
                return ApiResponse<List<AirportDto>>.ErrorResult("Failed to retrieve airports");
            }
        }

        public async Task<ApiResponse<StationDto>> CreateStationAsync(CreateStationDto createStationDto)
        {
            try
            {
                var station = new Station
                {
                    StationName = createStationDto.StationName,
                    StationCode = createStationDto.StationCode,
                    City = createStationDto.City,
                    State = createStationDto.State,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                };

                _context.Stations.Add(station);
                await _context.SaveChangesAsync();

                var stationDto = new StationDto
                {
                    StationID = station.StationID,
                    StationName = station.StationName,
                    StationCode = station.StationCode,
                    City = station.City,
                    State = station.State
                };

                return ApiResponse<StationDto>.SuccessResult(stationDto, "Station created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating station");
                return ApiResponse<StationDto>.ErrorResult("Failed to create station");
            }
        }

        public async Task<ApiResponse<StationDto>> UpdateStationAsync(int stationId, UpdateStationDto updateStationDto)
        {
            try
            {
                var station = await _context.Stations.FindAsync(stationId);
                if (station == null)
                {
                    return ApiResponse<StationDto>.ErrorResult("Station not found");
                }

                station.StationName = updateStationDto.StationName;
                station.StationCode = updateStationDto.StationCode;
                station.City = updateStationDto.City;
                station.State = updateStationDto.State;
                station.UpdatedAt = DateTime.UtcNow;
                station.UpdatedBy = "Admin";

                await _context.SaveChangesAsync();

                var stationDto = new StationDto
                {
                    StationID = station.StationID,
                    StationName = station.StationName,
                    StationCode = station.StationCode,
                    City = station.City,
                    State = station.State
                };

                return ApiResponse<StationDto>.SuccessResult(stationDto, "Station updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating station {StationId}", stationId);
                return ApiResponse<StationDto>.ErrorResult("Failed to update station");
            }
        }

        public async Task<ApiResponse> DeleteStationAsync(int stationId)
        {
            try
            {
                var station = await _context.Stations.FindAsync(stationId);
                if (station == null)
                {
                    return ApiResponse.ErrorResult("Station not found");
                }

                // Check if station is used by any trains
                var isUsed = await _context.Trains
                    .AnyAsync(t => t.SourceStationID == stationId || t.DestinationStationID == stationId);

                if (isUsed)
                {
                    return ApiResponse.ErrorResult("Cannot delete station as it is used by existing trains");
                }

                station.IsActive = false;
                station.UpdatedAt = DateTime.UtcNow;
                station.UpdatedBy = "Admin";

                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult("Station deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting station {StationId}", stationId);
                return ApiResponse.ErrorResult("Failed to delete station");
            }
        }

        public async Task<ApiResponse<List<CoachDto>>> GetTrainCoachesAsync(int trainId)
        {
            try
            {
                var coaches = await _context.Coaches
                    .Include(c => c.TrainClass)
                    .Include(c => c.Seats)
                    .Where(c => c.TrainID == trainId)
                    .Select(c => new CoachDto
                    {
                        CoachId = c.CoachID,
                        TrainId = c.TrainID,
                        CoachNumber = c.CoachNumber,
                        TrainClassId = c.TrainClassID,
                        TrainClassName = c.TrainClass.ClassName,
                        TotalSeats = c.TotalSeats,
                        AvailableSeats = c.AvailableSeats,
                        BaseFare = c.BaseFare,
                        Seats = c.Seats.Select(s => new SeatDto
                        {
                            SeatId = s.SeatID,
                            CoachId = s.CoachID,
                            SeatNumber = s.SeatNumber,
                            SeatType = s.SeatType,
                            IsAvailable = s.IsAvailable,
                            IsWindowSeat = s.IsWindowSeat
                        }).ToList()
                    })
                    .ToListAsync();

                return ApiResponse<List<CoachDto>>.SuccessResult(coaches, "Train coaches retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving coaches for train {TrainId}", trainId);
                return ApiResponse<List<CoachDto>>.ErrorResult("Failed to retrieve train coaches");
            }
        }

        public async Task<ApiResponse<CoachDto>> AddCoachToTrainAsync(int trainId, CreateCoachDto createCoachDto)
        {
            try
            {
                var coach = new Coach
                {
                    TrainID = trainId,
                    CoachNumber = createCoachDto.CoachNumber,
                    TrainClassID = createCoachDto.TrainClassId,
                    TotalSeats = createCoachDto.TotalSeats,
                    AvailableSeats = createCoachDto.TotalSeats,
                    BaseFare = createCoachDto.BaseFare,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                };

                _context.Coaches.Add(coach);
                await _context.SaveChangesAsync();

                // Create seats for the coach
                await CreateSeatsForCoachAsync(coach.CoachID, createCoachDto.TrainClassId, createCoachDto.TotalSeats);

                var coachDto = await GetCoachByIdAsync(coach.CoachID);
                return coachDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding coach to train {TrainId}", trainId);
                return ApiResponse<CoachDto>.ErrorResult("Failed to add coach to train");
            }
        }

        private async Task<ApiResponse<CoachDto>> GetCoachByIdAsync(int coachId)
        {
            var coach = await _context.Coaches
                .Include(c => c.TrainClass)
                .Include(c => c.Seats)
                .Where(c => c.CoachID == coachId)
                .Select(c => new CoachDto
                {
                    CoachId = c.CoachID,
                    TrainId = c.TrainID,
                    CoachNumber = c.CoachNumber,
                    TrainClassId = c.TrainClassID,
                    TrainClassName = c.TrainClass.ClassName,
                    TotalSeats = c.TotalSeats,
                    AvailableSeats = c.AvailableSeats,
                    BaseFare = c.BaseFare,
                    Seats = c.Seats.Select(s => new SeatDto
                    {
                        SeatId = s.SeatID,
                        CoachId = s.CoachID,
                        SeatNumber = s.SeatNumber,
                        SeatType = s.SeatType,
                        IsAvailable = s.IsAvailable,
                        IsWindowSeat = s.IsWindowSeat
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (coach == null)
            {
                return ApiResponse<CoachDto>.ErrorResult("Coach not found");
            }

            return ApiResponse<CoachDto>.SuccessResult(coach, "Coach retrieved successfully");
        }

        private async Task CreateSeatsForCoachAsync(int coachId, int trainClassId, int totalSeats)
        {
            var seats = new List<Seat>();

            // Create IRCTC-like seat layout based on train class
            switch (trainClassId)
            {
                case 1: // Sleeper Class (SL)
                    seats = CreateSleeperSeats(coachId, totalSeats);
                    break;
                case 2: // AC 3 Tier (3A)
                    seats = Create3TierACSeats(coachId, totalSeats);
                    break;
                case 3: // AC 2 Tier (2A)
                    seats = Create2TierACSeats(coachId, totalSeats);
                    break;
                case 4: // AC 1st Class (1A)
                    seats = CreateFirstClassSeats(coachId, totalSeats);
                    break;
                case 5: // Chair Car (CC)
                case 6: // Second Sitting (2S)
                    seats = CreateChairCarSeats(coachId, totalSeats);
                    break;
                default:
                    seats = CreateSleeperSeats(coachId, totalSeats);
                    break;
            }

            _context.Seats.AddRange(seats);
            await _context.SaveChangesAsync();
        }

        private List<Seat> CreateSleeperSeats(int coachId, int totalSeats)
        {
            var seats = new List<Seat>();
            for (int i = 1; i <= totalSeats; i++)
            {
                var seatType = (i % 8) switch
                {
                    1 or 4 => "Lower",
                    2 or 5 => "Middle",
                    3 or 6 => "Upper",
                    7 => "Side Lower",
                    0 => "Side Upper",
                    _ => "Lower"
                };

                seats.Add(new Seat
                {
                    CoachID = coachId,
                    SeatNumber = i.ToString(),
                    SeatType = seatType,
                    IsAvailable = true,
                    IsWindowSeat = (i % 8 == 1 || i % 8 == 6 || i % 8 == 7 || i % 8 == 0),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                });
            }
            return seats;
        }

        private List<Seat> Create3TierACSeats(int coachId, int totalSeats)
        {
            return CreateSleeperSeats(coachId, totalSeats); // Similar layout to sleeper
        }

        private List<Seat> Create2TierACSeats(int coachId, int totalSeats)
        {
            var seats = new List<Seat>();
            for (int i = 1; i <= totalSeats; i++)
            {
                var seatType = (i % 4) switch
                {
                    1 or 2 => "Lower",
                    3 or 0 => "Upper",
                    _ => "Lower"
                };

                seats.Add(new Seat
                {
                    CoachID = coachId,
                    SeatNumber = i.ToString(),
                    SeatType = seatType,
                    IsAvailable = true,
                    IsWindowSeat = (i % 4 == 1 || i % 4 == 0),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                });
            }
            return seats;
        }

        private List<Seat> CreateFirstClassSeats(int coachId, int totalSeats)
        {
            var seats = new List<Seat>();
            for (int i = 1; i <= totalSeats; i++)
            {
                seats.Add(new Seat
                {
                    CoachID = coachId,
                    SeatNumber = i.ToString(),
                    SeatType = "Cabin",
                    IsAvailable = true,
                    IsWindowSeat = true, // Most first class seats have window access
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                });
            }
            return seats;
        }

        private List<Seat> CreateChairCarSeats(int coachId, int totalSeats)
        {
            var seats = new List<Seat>();
            for (int i = 1; i <= totalSeats; i++)
            {
                seats.Add(new Seat
                {
                    CoachID = coachId,
                    SeatNumber = i.ToString(),
                    SeatType = "Chair",
                    IsAvailable = true,
                    IsWindowSeat = (i % 4 == 1 || i % 4 == 0),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin"
                });
            }
            return seats;
        }

        public async Task<ApiResponse<CoachDto>> UpdateCoachAsync(int coachId, UpdateCoachDto updateCoachDto)
        {
            try
            {
                var coach = await _context.Coaches.FindAsync(coachId);
                if (coach == null)
                {
                    return ApiResponse<CoachDto>.ErrorResult("Coach not found");
                }

                coach.CoachNumber = updateCoachDto.CoachNumber;
                coach.TrainClassID = updateCoachDto.TrainClassId;
                coach.TotalSeats = updateCoachDto.TotalSeats;
                coach.BaseFare = updateCoachDto.BaseFare;
                coach.UpdatedAt = DateTime.UtcNow;
                coach.UpdatedBy = "Admin";

                await _context.SaveChangesAsync();

                var coachDto = await GetCoachByIdAsync(coachId);
                return coachDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating coach {CoachId}", coachId);
                return ApiResponse<CoachDto>.ErrorResult("Failed to update coach");
            }
        }

        public async Task<ApiResponse> DeleteCoachAsync(int coachId)
        {
            try
            {
                var coach = await _context.Coaches.FindAsync(coachId);
                if (coach == null)
                {
                    return ApiResponse.ErrorResult("Coach not found");
                }

                coach.IsActive = false;
                coach.UpdatedAt = DateTime.UtcNow;
                coach.UpdatedBy = "Admin";

                await _context.SaveChangesAsync();

                return ApiResponse.SuccessResult("Coach deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting coach {CoachId}", coachId);
                return ApiResponse.ErrorResult("Failed to delete coach");
            }
        }
    }
}