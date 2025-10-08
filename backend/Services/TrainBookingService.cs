using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TransitHub.Data;
using TransitHub.Models;
using TransitHub.Models.DTOs;
using TransitHub.Repositories.Interfaces;
using TransitHub.Services.Interfaces;

namespace TransitHub.Services
{
    public class TrainBookingService : ITrainBookingService
    {
        private readonly TransitHubDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TrainBookingService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public TrainBookingService(
            TransitHubDbContext context,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            ILogger<TrainBookingService> logger,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request)
        {
            try
            {
                _logger.LogInformation("Creating booking for schedule {ScheduleId} with {PassengerCount} passengers", request.ScheduleId, request.PassengerCount);
                
                // Get or create user based on JWT token
                var currentUser = await GetOrCreateCurrentUserAsync();

                // Get train schedule
                var schedule = await _context.TrainSchedules
                    .Include(ts => ts.Train)
                    .ThenInclude(t => t.SourceStation)
                    .Include(ts => ts.Train)
                    .ThenInclude(t => t.DestinationStation)
                    .FirstOrDefaultAsync(ts => ts.ScheduleID == request.ScheduleId);

                if (schedule == null)
                {
                    return new BookingResponse
                    {
                        Success = false,
                        Message = "Train schedule not found"
                    };
                }

                // Allow booking until departure (no 30-minute restriction)

                // Check availability - if no seats, add to waitlist
                bool isWaitlisted = schedule.AvailableSeats < request.PassengerCount;
                int statusId = isWaitlisted ? 2 : 1; // 2 = Waitlisted, 1 = Confirmed

                // Calculate quota-based pricing
                decimal finalFare = CalculateQuotaBasedFare(schedule.Fare, request.SelectedQuota, request.Passengers);

                // Create booking
                var booking = new Booking
                {
                    UserID = currentUser.UserID,
                    BookingReference = GenerateBookingReference(),
                    BookingType = "Train",
                    TrainScheduleID = request.ScheduleId,
                    TotalPassengers = request.PassengerCount,
                    TotalAmount = finalFare * request.PassengerCount,
                    BookingDate = DateTime.UtcNow,
                    StatusID = statusId
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created booking {BookingId} with reference {BookingReference}", booking.BookingID, booking.BookingReference);

                // Create passengers with proper seat assignment
                var seatAllocations = new List<SeatAllocation>();
                var assignedSeats = new List<int>();
                
                if (!isWaitlisted)
                {
                    // Handle confirmed booking with seat assignment
                    var preferredSeats = request.PreferredSeats ?? new List<int>();
                    var autoAssign = request.AutoAssignSeats;
                    

                    
                    // If specific seats are requested, validate they're all available first
                    if (!autoAssign && preferredSeats.Count > 0)
                    {
                        if (preferredSeats.Count != request.Passengers.Count)
                        {
                            return new BookingResponse
                            {
                                Success = false,
                                Message = "Number of preferred seats must match number of passengers"
                            };
                        }
                        
                        // Check if any preferred seats are already occupied
                        var occupiedSeats = new List<int>();
                        foreach (var seatNum in preferredSeats)
                        {
                            if (IsSeatOccupied(schedule.ScheduleID, seatNum))
                            {
                                occupiedSeats.Add(seatNum);
                            }
                        }
                        
                        if (occupiedSeats.Count > 0)
                        {
                            return new BookingResponse
                            {
                                Success = false,
                                Message = $"Selected seats are already occupied: {string.Join(", ", occupiedSeats)}"
                            };
                        }
                    }
                    
                    for (int i = 0; i < request.Passengers.Count; i++)
                    {
                        var passenger = request.Passengers[i];
                        int seatNumber;
                        

                        
                        if (!autoAssign && preferredSeats.Count > i)
                        {
                            // Use the exact preferred seat (already validated as available)
                            seatNumber = preferredSeats[i];

                        }
                        else
                        {
                            // Auto-assign seat
                            seatNumber = GetNextAvailableSeatNumber(schedule.ScheduleID, assignedSeats);

                        }
                        
                        assignedSeats.Add(seatNumber);

                        var bookingPassenger = new BookingPassenger
                        {
                            BookingID = booking.BookingID,
                            Name = passenger.Name,
                            Age = passenger.Age,
                            Gender = passenger.Gender,
                            IsSeniorCitizen = passenger.Age >= 58,
                            SeatNumber = seatNumber.ToString()
                        };

                        _context.BookingPassengers.Add(bookingPassenger);


                        seatAllocations.Add(new SeatAllocation
                        {
                            PassengerId = bookingPassenger.PassengerID,
                            PassengerName = passenger.Name,
                            CoachNumber = "S1",
                            SeatNumber = seatNumber,
                            Status = "Confirmed",
                            Priority = 1
                        });
                    }

                    // Update available seats for confirmed bookings
                    schedule.AvailableSeats -= request.PassengerCount;
                }
                else
                {
                    // Handle waitlisted booking - no seat assignment yet
                    for (int i = 0; i < request.Passengers.Count; i++)
                    {
                        var passenger = request.Passengers[i];

                        var bookingPassenger = new BookingPassenger
                        {
                            BookingID = booking.BookingID,
                            Name = passenger.Name,
                            Age = passenger.Age,
                            Gender = passenger.Gender,
                            IsSeniorCitizen = passenger.Age >= 58,
                            SeatNumber = string.Empty // No seat assigned for waitlisted passengers
                        };

                        _context.BookingPassengers.Add(bookingPassenger);

                        seatAllocations.Add(new SeatAllocation
                        {
                            PassengerId = bookingPassenger.PassengerID,
                            PassengerName = passenger.Name,
                            CoachNumber = "WL", // Waitlist coach
                            SeatNumber = 0, // No seat number for waitlist
                            Status = "Waitlisted",
                            Priority = passenger.Age >= 58 ? 1 : 2 // Senior citizens get priority
                        });
                    }
                    
                    // Add to waitlist (save booking first to get BookingID)
                    await _context.SaveChangesAsync();
                    await AddToWaitlistAsync(booking, request.Passengers);
                }
                
                await _context.SaveChangesAsync();

                // Create notification
                var trainName = schedule.Train?.TrainName ?? "Train";
                if (isWaitlisted)
                {
                    await _notificationService.CreateNotificationAsync(
                        currentUser.UserID,
                        "Booking Added to Waitlist",
                        $"Your booking for {trainName} (PNR: {booking.BookingReference}) has been added to the waitlist.",
                        "waitlist",
                        booking.BookingID
                    );
                    _logger.LogInformation("Successfully created waitlisted booking {BookingId} with {PassengerCount} passengers", 
                        booking.BookingID, request.PassengerCount);
                }
                else
                {
                    await _notificationService.CreateNotificationAsync(
                        currentUser.UserID,
                        "Booking Confirmed",
                        $"Your booking for {trainName} (PNR: {booking.BookingReference}) has been confirmed successfully.",
                        "confirmed",
                        booking.BookingID
                    );
                    _logger.LogInformation("Successfully created confirmed booking {BookingId} with {SeatCount} seats", 
                        booking.BookingID, assignedSeats.Count);
                    
                    // Send booking confirmation email
                    await SendBookingConfirmationEmail(currentUser.UserID, booking.BookingReference, booking.TotalAmount);
                }
                
                return new BookingResponse
                {
                    Success = true,
                    Message = isWaitlisted ? "Booking added to waitlist successfully" : "Booking confirmed successfully",
                    Data = new BookingData
                    {
                        BookingId = booking.BookingID,
                        Pnr = booking.BookingReference,
                        Status = isWaitlisted ? "Waitlisted" : "Confirmed",
                        SeatAllocations = seatAllocations
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking for schedule {ScheduleId}: {Error}", request.ScheduleId, ex.Message);
                return new BookingResponse
                {
                    Success = false,
                    Message = "An error occurred while creating the booking: " + ex.Message
                };
            }
        }

        public async Task<CancelBookingResponse> CancelBookingAsync(CancelBookingRequest request)
        {
            try
            {
                var currentUser = await GetOrCreateCurrentUserAsync();
                var booking = await _context.Bookings
                    .Include(b => b.TrainSchedule)
                    .FirstOrDefaultAsync(b => b.BookingID == request.BookingId && b.UserID == currentUser.UserID);

                if (booking == null)
                {
                    return new CancelBookingResponse
                    {
                        Success = false,
                        Message = "Booking not found or you don't have permission to cancel it"
                    };
                }

                if (booking.StatusID == 3) // Assuming 3 is Cancelled status
                {
                    return new CancelBookingResponse
                    {
                        Success = false,
                        Message = "Booking is already cancelled"
                    };
                }

                // Calculate refund based on timing
                decimal refundPercentage = 0.8m; // Default 80% refund
                if (booking.TrainSchedule != null)
                {
                    var departureDateTime = booking.TrainSchedule.TravelDate.ToDateTime(TimeOnly.FromDateTime(booking.TrainSchedule.DepartureTime));
                    var currentDateTime = DateTime.Now;
                    var timeDifference = departureDateTime - currentDateTime;
                    
                    if (timeDifference.TotalMinutes < 0)
                    {
                        return new CancelBookingResponse
                        {
                            Success = false,
                            Message = "Cannot cancel booking. Train has already departed."
                        };
                    }
                    
                    // Late cancellation penalty: <30 min = 40% refund, >30 min = 80% refund
                    refundPercentage = timeDifference.TotalMinutes < 30 ? 0.4m : 0.8m;
                }

                // Store original status before changing it
                var originalStatusId = booking.StatusID;
                
                // Clear seat numbers for cancelled booking passengers
                var passengers = await _context.BookingPassengers
                    .Where(p => p.BookingID == booking.BookingID)
                    .ToListAsync();
                
                foreach (var passenger in passengers)
                {
                    if (!string.IsNullOrEmpty(passenger.SeatNumber))
                    {
                        _logger.LogInformation("Clearing seat {SeatNumber} for cancelled passenger {PassengerName}", 
                            passenger.SeatNumber, passenger.Name);
                        passenger.SeatNumber = string.Empty;
                    }
                }
                
                // Update booking status
                booking.StatusID = 3; // Cancelled
                
                // Update available seats in schedule and process waitlist
                if (booking.TrainSchedule != null && originalStatusId == 1) // Only if it was confirmed
                {
                    booking.TrainSchedule.AvailableSeats += booking.TotalPassengers;
                    _logger.LogInformation("Released {PassengerCount} seats for schedule {ScheduleId}, new available: {AvailableSeats}", 
                        booking.TotalPassengers, booking.TrainScheduleID, booking.TrainSchedule.AvailableSeats);
                    
                    // Process waitlist to confirm next passengers
                    if (booking.TrainScheduleID.HasValue)
                    {
                        await ProcessWaitlistAsync(booking.TrainScheduleID.Value, booking.TotalPassengers);
                    }
                }

                await _context.SaveChangesAsync();

                // Create cancellation notification
                var trainName = booking.TrainSchedule?.Train?.TrainName ?? "Train";
                await _notificationService.CreateNotificationAsync(
                    currentUser.UserID,
                    "Booking Cancelled",
                    $"Your booking for {trainName} (PNR: {booking.BookingReference}) has been cancelled. Refund: ₹{booking.TotalAmount * refundPercentage:F2}",
                    "cancelled",
                    booking.BookingID
                );
                
                // Send cancellation confirmation email
                await SendCancellationConfirmationEmail(currentUser.UserID, booking.BookingReference, booking.TotalAmount * refundPercentage);

                return new CancelBookingResponse
                {
                    Success = true,
                    Message = refundPercentage == 0.4m ? 
                        "Booking cancelled with late cancellation penalty" : 
                        "Booking cancelled successfully",
                    RefundAmount = booking.TotalAmount * refundPercentage
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking: {Error}", ex.Message);
                return new CancelBookingResponse
                {
                    Success = false,
                    Message = "An error occurred while cancelling the booking: " + ex.Message
                };
            }
        }

        public async Task<CoachLayoutResponse> GetCoachLayoutAsync(int scheduleId, string trainClass)
        {
            try
            {
                _logger.LogInformation("Getting coach layout for schedule {ScheduleId}, class {TrainClass}", scheduleId, trainClass);
                
                var schedule = await _context.TrainSchedules
                    .FirstOrDefaultAsync(ts => ts.ScheduleID == scheduleId);

                if (schedule == null)
                {
                    _logger.LogWarning("Schedule {ScheduleId} not found", scheduleId);
                    return new CoachLayoutResponse();
                }

                // Get actually booked seats for this schedule - only confirmed bookings with valid seat numbers
                var bookedSeatNumbers = await _context.BookingPassengers
                    .Where(p => p.Booking.TrainScheduleID == scheduleId && 
                               p.Booking.StatusID == 1 && // Only confirmed bookings (not cancelled)
                               !string.IsNullOrEmpty(p.SeatNumber) &&
                               p.SeatNumber != "null") // Exclude null strings
                    .Select(p => p.SeatNumber)
                    .ToListAsync();
                

                
                // Also check for any cancelled bookings with seat numbers that need cleanup
                var cancelledWithSeats = await _context.BookingPassengers
                    .Where(p => p.Booking.TrainScheduleID == scheduleId && 
                               p.Booking.StatusID == 3 && // Cancelled bookings
                               !string.IsNullOrEmpty(p.SeatNumber))
                    .Select(p => new { p.PassengerID, p.SeatNumber, p.Name })
                    .ToListAsync();
                    
                if (cancelledWithSeats.Any())
                {
                    _logger.LogWarning("Found {Count} cancelled passengers with seat numbers: {Seats}", 
                        cancelledWithSeats.Count, string.Join(", ", cancelledWithSeats.Select(p => $"{p.Name}:{p.SeatNumber}")));
                    
                    // Clear these seat numbers
                    foreach (var cancelled in cancelledWithSeats)
                    {
                        var passenger = await _context.BookingPassengers.FindAsync(cancelled.PassengerID);
                        if (passenger != null)
                        {
                            passenger.SeatNumber = string.Empty;
                        }
                    }
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Cleared seat numbers from {Count} cancelled passengers", cancelledWithSeats.Count);
                }
                
                var bookedSeats = bookedSeatNumbers
                    .Where(seatStr => !string.IsNullOrEmpty(seatStr) && int.TryParse(seatStr, out int seatNum) && seatNum > 0)
                    .Select(seatStr => int.Parse(seatStr!))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();


                
                // Use database available seats (which is correct) and show only that many seats as available
                var dbAvailableSeats = schedule.AvailableSeats;
                var actualBookedSeats = bookedSeats.Count;
                


                var totalSeats = schedule.TotalSeats;
                var seats = new List<SeatInfo>();
                
                // Create seats and mark the actual booked seat numbers as occupied
                for (int i = 1; i <= totalSeats; i++)
                {
                    var isOccupied = bookedSeats.Contains(i);
                    seats.Add(new SeatInfo
                    {
                        SeatNumber = i,
                        IsOccupied = isOccupied
                    });
                }

                var coaches = new List<CoachLayout>
                {
                    new CoachLayout
                    {
                        CoachNumber = 1,
                        TotalSeats = totalSeats,
                        AvailableSeats = dbAvailableSeats,
                        Seats = seats
                    }
                };

                return new CoachLayoutResponse
                {
                    Coaches = coaches
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting coach layout for schedule {ScheduleId}: {Error}", scheduleId, ex.Message);
                return new CoachLayoutResponse();
            }
        }

        private int GetNextAvailableSeatNumber(int scheduleId, List<int> alreadyAssigned = null)
        {
            try
            {
                alreadyAssigned = alreadyAssigned ?? new List<int>();
                
                // Get all booked seat numbers for this schedule
                var bookedSeatNumbers = _context.BookingPassengers
                    .Where(p => p.Booking.TrainScheduleID == scheduleId && 
                               p.Booking.StatusID == 1 && 
                               !string.IsNullOrEmpty(p.SeatNumber))
                    .Select(p => p.SeatNumber)
                    .ToList();
                
                var bookedSeats = bookedSeatNumbers
                    .Where(seatStr => !string.IsNullOrEmpty(seatStr) && int.TryParse(seatStr, out int seatNum) && seatNum > 0)
                    .Select(seatStr => int.Parse(seatStr!))
                    .ToList();

                // Combine booked seats with already assigned seats in this booking
                var unavailableSeats = bookedSeats.Concat(alreadyAssigned).ToHashSet();

                // Find first available seat number
                for (int i = 1; i <= 100; i++) // Assuming max 100 seats
                {
                    if (!unavailableSeats.Contains(i))
                    {
                        return i;
                    }
                }
                
                return 1; // Fallback
            }
            catch
            {
                return 1; // Fallback
            }
        }
        
        private bool IsSeatOccupied(int scheduleId, int seatNumber)
        {
            try
            {
                return _context.BookingPassengers
                    .Where(p => p.Booking.TrainScheduleID == scheduleId && 
                               p.Booking.StatusID == 1 && 
                               p.SeatNumber == seatNumber.ToString())
                    .Any();
            }
            catch
            {
                return false;
            }
        }

        public async Task<BookingDetails> GetBookingDetailsAsync(int bookingId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var currentUser = await GetOrCreateCurrentUserAsync();
                var booking = await _context.Bookings
                    .Include(b => b.Passengers)
                    .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.UserID == currentUser.UserID);

                if (booking == null)
                {
                    throw new ArgumentException("Booking not found");
                }

                // Get train schedule details
                var schedule = await _context.TrainSchedules
                    .Include(ts => ts.Train)
                    .ThenInclude(t => t.SourceStation)
                    .Include(ts => ts.Train)
                    .ThenInclude(t => t.DestinationStation)
                    .FirstOrDefaultAsync(ts => ts.ScheduleID == booking.TrainScheduleID);

                // Get waitlist position if booking is waitlisted
                int? waitlistPosition = null;
                if (booking.StatusID == 2) // Waitlisted
                {
                    var waitlistEntry = await _context.WaitlistQueues
                        .FirstOrDefaultAsync(w => w.BookingID == booking.BookingID && w.IsActive);
                    waitlistPosition = waitlistEntry?.Position;
                }

                return new BookingDetails
                {
                    BookingId = booking.BookingID,
                    BookingReference = booking.BookingReference,
                    Status = GetStatusName(booking.StatusID),
                    TotalAmount = booking.TotalAmount,
                    PaymentStatus = "Paid",
                    TrainName = schedule?.Train?.TrainName ?? "Unknown Train",
                    TrainNumber = schedule?.Train?.TrainNumber ?? "N/A",
                    Route = $"{schedule?.Train?.SourceStation?.StationName} - {schedule?.Train?.DestinationStation?.StationName}",
                    TravelDate = schedule?.TravelDate.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd"),
                    DepartureTime = schedule?.DepartureTime.ToString("HH:mm") ?? "N/A",
                    ArrivalTime = schedule?.ArrivalTime.ToString("HH:mm") ?? "N/A",
                    WaitlistPosition = waitlistPosition,
                    Passengers = booking.Passengers.Select(p => new PassengerDetails
                    {
                        Name = p.Name,
                        Age = p.Age,
                        Gender = p.Gender,
                        SeatNumber = int.TryParse(p.SeatNumber, out int seat) ? seat : null
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking details");
                throw;
            }
        }

        public async Task<List<BookingDetails>> GetUserBookingsAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return new List<BookingDetails>();
                }

                var currentUser = await GetOrCreateCurrentUserAsync();
                var bookings = await _context.Bookings
                    .Include(b => b.Passengers)
                    .Where(b => b.UserID == currentUser.UserID && b.IsActive)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();

                var bookingDetailsList = new List<BookingDetails>();
                
                foreach (var booking in bookings)
                {
                    var schedule = await _context.TrainSchedules
                        .Include(ts => ts.Train)
                        .ThenInclude(t => t.SourceStation)
                        .Include(ts => ts.Train)
                        .ThenInclude(t => t.DestinationStation)
                        .Include(ts => ts.TrainClass)
                        .FirstOrDefaultAsync(ts => ts.ScheduleID == booking.TrainScheduleID);
                    
                    // Get waitlist position if booking is waitlisted
                    int? waitlistPosition = null;
                    if (booking.StatusID == 2) // Waitlisted
                    {
                        var waitlistEntry = await _context.WaitlistQueues
                            .FirstOrDefaultAsync(w => w.BookingID == booking.BookingID && w.IsActive);
                        waitlistPosition = waitlistEntry?.Position;
                    }

                    bookingDetailsList.Add(new BookingDetails
                    {
                        BookingId = booking.BookingID,
                        BookingReference = booking.BookingReference,
                        Status = GetStatusName(booking.StatusID),
                        TotalAmount = booking.TotalAmount,
                        PaymentStatus = "Paid",
                        TrainName = schedule?.Train?.TrainName ?? "Unknown Train",
                        TrainNumber = schedule?.Train?.TrainNumber ?? "N/A",
                        Route = schedule != null ? $"{schedule.Train?.SourceStation?.StationName} - {schedule.Train?.DestinationStation?.StationName}" : "Unknown Route",
                        TravelDate = schedule?.TravelDate.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd"),
                        DepartureTime = schedule?.DepartureTime.ToString("HH:mm") ?? "N/A",
                        ArrivalTime = schedule?.ArrivalTime.ToString("HH:mm") ?? "N/A",
                        TrainClass = schedule?.TrainClass?.ClassName ?? "Sleeper",
                        WaitlistPosition = waitlistPosition,
                        Passengers = booking.Passengers.Select(p => new PassengerDetails
                        {
                            Name = p.Name,
                            Age = p.Age,
                            Gender = p.Gender,
                            SeatNumber = int.TryParse(p.SeatNumber, out int seat) ? seat : null
                        }).ToList()
                    });
                }
                
                return bookingDetailsList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookings");
                return new List<BookingDetails>();
            }
        }

        public Task<AddCoachResponse> AddCoachesAsync(int trainId, AddCoachRequest request)
        {
            try
            {
                // This would implement the coach addition logic
                // For now, return success
                return Task.FromResult(new AddCoachResponse
                {
                    Success = true,
                    Message = "Coaches added successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding coaches");
                return Task.FromResult(new AddCoachResponse
                {
                    Success = false,
                    Message = "An error occurred while adding coaches"
                });
            }
        }

        public async Task<WaitlistInfo> GetWaitlistInfoAsync(int scheduleId)
        {
            try
            {
                var currentUser = await GetOrCreateCurrentUserAsync();
                
                // Get user's waitlist entry if exists
                var userWaitlistEntry = await _context.WaitlistQueues
                    .Include(w => w.Booking)
                    .FirstOrDefaultAsync(w => w.TrainScheduleID == scheduleId && 
                                            w.Booking.UserID == currentUser.UserID && 
                                            w.IsActive);
                
                if (userWaitlistEntry == null)
                {
                    var totalWaiting = await _context.WaitlistQueues
                        .CountAsync(w => w.TrainScheduleID == scheduleId && w.IsActive);
                    
                    return new WaitlistInfo
                    {
                        Position = totalWaiting + 1,
                        TotalWaiting = totalWaiting,
                        EstimatedConfirmation = "2-3 days"
                    };
                }
                
                // Calculate actual position considering priority
                var seniorCount = await _context.WaitlistQueues
                    .Where(w => w.TrainScheduleID == scheduleId && w.IsActive && w.Priority == 1 && 
                               (w.Priority < userWaitlistEntry.Priority || 
                                (w.Priority == userWaitlistEntry.Priority && w.Position < userWaitlistEntry.Position)))
                    .CountAsync();
                
                var regularCount = userWaitlistEntry.Priority == 2 ? 
                    await _context.WaitlistQueues
                        .Where(w => w.TrainScheduleID == scheduleId && w.IsActive && w.Priority == 2 && 
                                   w.Position < userWaitlistEntry.Position)
                        .CountAsync() : 0;
                
                var actualPosition = seniorCount + regularCount + 1;
                var totalWaitingCount = await _context.WaitlistQueues
                    .CountAsync(w => w.TrainScheduleID == scheduleId && w.IsActive);
                
                return new WaitlistInfo
                {
                    Position = actualPosition,
                    TotalWaiting = totalWaitingCount,
                    EstimatedConfirmation = actualPosition <= 10 ? "1-2 days" : "3-5 days"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting waitlist info");
                return new WaitlistInfo();
            }
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }



        private string GenerateBookingReference()
        {
            return "PNR" + DateTime.Now.Ticks.ToString().Substring(10);
        }

        private async Task<List<SeatInfo>> GetActualSeatsAsync(int trainId, int coachNumber)
        {
            var seats = await _context.Seats
                .Include(s => s.Coach)
                .Where(s => s.Coach.TrainID == trainId && s.Coach.CoachNumber == coachNumber.ToString())
                .Select(s => new SeatInfo
                {
                    SeatNumber = int.Parse(s.SeatNumber),
                    IsOccupied = !s.IsAvailable
                })
                .ToListAsync();

            return seats;
        }

        private async Task<User> GetOrCreateCurrentUserAsync()
        {
            var jwtUserId = GetCurrentUserId();
            
            // Debug: Log all available claims
            var allClaims = _httpContextAccessor.HttpContext?.User?.Claims?.Select(c => $"{c.Type}={c.Value}").ToList() ?? new List<string>();
            _logger.LogWarning("DEBUG: All JWT Claims: [{Claims}]", string.Join(", ", allClaims));
            
            if (string.IsNullOrEmpty(jwtUserId))
            {
                _logger.LogWarning("No JWT user ID found, using anonymous user");
                return await GetOrCreateAnonymousUserAsync();
            }

            // Try to find user by email from JWT claims (try multiple claim types)
            var email = _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value
                     ?? _httpContextAccessor.HttpContext?.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
                     ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
            
            var name = _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;
            
            _logger.LogInformation("JWT User ID: {UserId}, Email: {Email}, Name: {Name}", jwtUserId, email, name);
            
            if (!string.IsNullOrEmpty(email))
            {
                // Use actual email as unique identifier (one user per email, regardless of role)
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    _logger.LogInformation("Found existing user {UserId} with email {Email}", existingUser.UserID, email);
                    return existingUser;
                }

                // Create new user with actual email
                var newUser = new User
                {
                    Name = name ?? "User",
                    Email = email, // Use actual email
                    PasswordHash = "jwt_user",
                    Phone = "0000000000",
                    Age = 25,
                    IsSeniorCitizen = false,
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedBy = "System"
                };
                
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created new user {UserId} with email {Email}", newUser.UserID, email);
                return newUser;
            }

            // Fallback to anonymous user
            _logger.LogWarning("No email in JWT claims, using anonymous user - THIS IS WHY ALL USERS SEE SAME BOOKINGS!");
            return await GetOrCreateAnonymousUserAsync();
        }

        private async Task<User> GetOrCreateAnonymousUserAsync()
        {
            var anonymousUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "anonymous@transithub.com");
            if (anonymousUser == null)
            {
                anonymousUser = new User
                {
                    Name = "Anonymous User",
                    Email = "anonymous@transithub.com",
                    PasswordHash = "anonymous",
                    Phone = "0000000000",
                    Age = 30,
                    IsSeniorCitizen = false,
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                
                _context.Users.Add(anonymousUser);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created anonymous user {UserId}", anonymousUser.UserID);
            }
            return anonymousUser;
        }

        private string GetStatusName(int statusId)
        {
            return statusId switch
            {
                1 => "Confirmed",
                2 => "Waitlisted",
                3 => "Cancelled",
                _ => "Unknown"
            };
        }

        private async Task AddToWaitlistAsync(Booking booking, List<PassengerInfo> passengers)
        {
            // Only handle train bookings for waitlist
            if (!booking.TrainScheduleID.HasValue)
            {
                _logger.LogWarning("Cannot add booking {BookingId} to waitlist - no train schedule ID", booking.BookingID);
                return;
            }
            
            // Get TrainClass from TrainSchedule
            var trainClass = await _context.TrainSchedules
                .Where(ts => ts.ScheduleID == booking.TrainScheduleID.Value)
                .Join(_context.TrainClasses, ts => ts.TrainClassID, tc => tc.TrainClassID, (ts, tc) => tc.ClassName)
                .FirstOrDefaultAsync();
            
            // Determine priority: 1 = Senior Citizen (58+), 2 = Regular
            bool hasSeniorCitizen = passengers.Any(p => p.Age >= 58);
            int priority = hasSeniorCitizen ? 1 : 2;
            
            // Get current position in waitlist for this priority and class
            var currentPosition = await _context.WaitlistQueues
                .Where(w => w.TrainScheduleID == booking.TrainScheduleID.Value && w.IsActive && w.Priority == priority && w.TrainClass == trainClass)
                .CountAsync();
            
            var waitlistEntry = new WaitlistQueue
            {
                BookingID = booking.BookingID,
                TrainScheduleID = booking.TrainScheduleID.Value,
                Position = currentPosition + 1,
                Priority = priority,
                TrainClass = trainClass,
                QueuedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };
            
            _context.WaitlistQueues.Add(waitlistEntry);
            _logger.LogInformation("Added booking {BookingId} to waitlist at position {Position} with priority {Priority} for class {TrainClass}", 
                booking.BookingID, waitlistEntry.Position, priority, trainClass);
        }

        private async Task ProcessWaitlistAsync(int scheduleId, int availableSeats)
        {
            // Get waitlist entries ordered by priority (1 = Senior, 2 = Regular) then by position
            var waitlistEntries = await _context.WaitlistQueues
                .Include(w => w.Booking)
                .ThenInclude(b => b.Passengers)
                .Where(w => w.TrainScheduleID == scheduleId && w.IsActive)
                .OrderBy(w => w.Priority)
                .ThenBy(w => w.Position)
                .ToListAsync();
            
            int seatsToAllocate = availableSeats;
            int seatsAllocated = 0;
            
            foreach (var waitlistEntry in waitlistEntries)
            {
                if (seatsToAllocate <= 0) break;
                
                var booking = waitlistEntry.Booking;
                if (booking.TotalPassengers <= seatsToAllocate)
                {
                    // Confirm this booking
                    booking.StatusID = 1; // Confirmed
                    waitlistEntry.IsActive = false; // Remove from waitlist
                    waitlistEntry.ConfirmedAt = DateTime.UtcNow;
                    
                    // Assign seats to passengers
                    var assignedSeats = new List<int>();
                    foreach (var passenger in booking.Passengers)
                    {
                        var seatNumber = GetNextAvailableSeatNumber(scheduleId, assignedSeats);
                        passenger.SeatNumber = seatNumber.ToString();
                        assignedSeats.Add(seatNumber);
                        _logger.LogInformation("Assigned seat {SeatNumber} to passenger {PassengerName} in booking {BookingId}", 
                            seatNumber, passenger.Name, booking.BookingID);
                    }
                    
                    seatsToAllocate -= booking.TotalPassengers;
                    seatsAllocated += booking.TotalPassengers;
                    
                    // Create waitlist promotion notification
                    var trainName = booking.TrainSchedule?.Train?.TrainName ?? "Train";
                    await _notificationService.CreateNotificationAsync(
                        booking.UserID,
                        "Waitlist Confirmed!",
                        $"Great news! Your waitlisted booking for {trainName} (PNR: {booking.BookingReference}) has been confirmed.",
                        "confirmed",
                        booking.BookingID
                    );
                    
                    _logger.LogInformation("Confirmed waitlisted booking {BookingId} with {PassengerCount} passengers", 
                        booking.BookingID, booking.TotalPassengers);
                }
            }
            
            // Update available seats - reduce by the number of seats allocated
            var schedule = await _context.TrainSchedules.FindAsync(scheduleId);
            if (schedule != null)
            {
                schedule.AvailableSeats -= seatsAllocated;
                _logger.LogInformation("Updated schedule {ScheduleId}: allocated {SeatsAllocated} seats, new available: {AvailableSeats}", 
                    scheduleId, seatsAllocated, schedule.AvailableSeats);
            }
            
            // Reorder remaining waitlist positions
            await ReorderWaitlistAsync(scheduleId);
        }
        
        private async Task ReorderWaitlistAsync(int scheduleId)
        {
            var activeWaitlist = await _context.WaitlistQueues
                .Where(w => w.TrainScheduleID == scheduleId && w.IsActive)
                .OrderBy(w => w.Priority)
                .ThenBy(w => w.QueuedAt)
                .ToListAsync();
            
            // Reorder positions within each priority group
            var seniorEntries = activeWaitlist.Where(w => w.Priority == 1).ToList();
            var regularEntries = activeWaitlist.Where(w => w.Priority == 2).ToList();
            
            for (int i = 0; i < seniorEntries.Count; i++)
            {
                seniorEntries[i].Position = i + 1;
            }
            
            for (int i = 0; i < regularEntries.Count; i++)
            {
                regularEntries[i].Position = i + 1;
            }
            
            _logger.LogInformation("Reordered waitlist for schedule {ScheduleId}: {SeniorCount} senior, {RegularCount} regular", 
                scheduleId, seniorEntries.Count, regularEntries.Count);
        }

        public async Task RemoveBookingAsync(int bookingId)
        {
            var currentUser = await GetOrCreateCurrentUserAsync();
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.UserID == currentUser.UserID);
            
            if (booking != null)
            {
                booking.IsActive = false;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Removed booking {BookingId} for user {UserId}", bookingId, currentUser.UserID);
            }
        }

        public async Task RemoveMultipleBookingsAsync(List<int> bookingIds)
        {
            var currentUser = await GetOrCreateCurrentUserAsync();
            var bookings = await _context.Bookings
                .Where(b => bookingIds.Contains(b.BookingID) && b.UserID == currentUser.UserID)
                .ToListAsync();
            
            foreach (var booking in bookings)
            {
                booking.IsActive = false;
            }
            
            await _context.SaveChangesAsync();
            _logger.LogInformation("Removed {Count} bookings for user {UserId}", bookings.Count, currentUser.UserID);
        }

        private async Task SendBookingConfirmationEmail(int userId, string bookingReference, decimal totalAmount)
        {
            try
            {
                _logger.LogInformation("Attempting to send booking confirmation email for user {UserId}, booking {BookingReference}", userId, bookingReference);
                
                var user = await _context.Users.FindAsync(userId);
                _logger.LogInformation("Retrieved user: {UserEmail}, Name: {UserName}", user?.Email, user?.Name);
                
                if (user?.Email != null)
                {
                    _logger.LogInformation("Calling email service for booking confirmation to {Email}", user.Email);
                    var result = await _emailService.SendBookingConfirmationEmailAsync(
                        user.Email, 
                        user.Name, 
                        bookingReference, 
                        "Train Booking", 
                        totalAmount
                    );
                    _logger.LogInformation("Email service returned: {Result} for booking {BookingReference}", result, bookingReference);
                }
                else
                {
                    _logger.LogWarning("User email is null for user {UserId}, cannot send confirmation email", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send booking confirmation email for booking {BookingReference}", bookingReference);
            }
        }

        private async Task SendCancellationConfirmationEmail(int userId, string bookingReference, decimal refundAmount)
        {
            try
            {
                _logger.LogInformation("Attempting to send cancellation email for user {UserId}, booking {BookingReference}", userId, bookingReference);
                
                var user = await _context.Users.FindAsync(userId);
                _logger.LogInformation("Retrieved user: {UserEmail}, Name: {UserName}", user?.Email, user?.Name);
                
                if (user?.Email != null)
                {
                    _logger.LogInformation("Calling email service for cancellation to {Email}", user.Email);
                    var result = await _emailService.SendBookingCancellationEmailAsync(
                        user.Email, 
                        user.Name, 
                        bookingReference, 
                        "Train Booking", 
                        refundAmount
                    );
                    _logger.LogInformation("Cancellation email service returned: {Result} for booking {BookingReference}", result, bookingReference);
                }
                else
                {
                    _logger.LogWarning("User email is null for user {UserId}, cannot send cancellation email", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cancellation confirmation email for booking {BookingReference}", bookingReference);
            }
        }

        private decimal CalculateQuotaBasedFare(decimal baseFare, string? selectedQuota, List<PassengerInfo> passengers)
        {
            if (string.IsNullOrEmpty(selectedQuota) || selectedQuota == "General")
            {
                return baseFare;
            }

            return selectedQuota switch
            {
                "Ladies" => baseFare * 0.95m, // 5% discount for ladies quota
                "Senior" => baseFare * 0.90m, // 10% discount for senior citizens
                "Tatkal" => baseFare * 1.30m, // 30% premium for Tatkal
                _ => baseFare
            };
        }
    }
}