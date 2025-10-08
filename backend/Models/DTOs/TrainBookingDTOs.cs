using System.ComponentModel.DataAnnotations;

namespace TransitHub.Models.DTOs
{
    public class CreateBookingRequest
    {
        [Required]
        public int TrainId { get; set; }
        
        [Required]
        public int ScheduleId { get; set; }
        
        [Required]
        public string TravelDate { get; set; } = string.Empty;
        
        [Required]
        public List<PassengerInfo> Passengers { get; set; } = new List<PassengerInfo>();
        
        [Required]
        public int PassengerCount { get; set; }
        
        public bool AutoAssignSeats { get; set; } = true;
        
        public List<int> PreferredSeats { get; set; } = new List<int>();
        
        public string? SelectedQuota { get; set; }
    }

    public class PassengerInfo
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 120)]
        public int Age { get; set; }
        
        [Required]
        public string Gender { get; set; } = string.Empty;
        
        public string? SeatPreference { get; set; }
    }

    public class BookingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public BookingData? Data { get; set; }
    }

    public class BookingData
    {
        public int BookingId { get; set; }
        public string Pnr { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<SeatAllocation> SeatAllocations { get; set; } = new List<SeatAllocation>();
    }

    public class SeatAllocation
    {
        public int PassengerId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string CoachNumber { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public class CancelBookingRequest
    {
        [Required]
        public int BookingId { get; set; }
        
        public string? Reason { get; set; }
    }

    public class CancelBookingResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal? RefundAmount { get; set; }
    }

    public class BookingDetails
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? CoachNumber { get; set; }
        public List<int> SeatNumbers { get; set; } = new List<int>();
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public int? WaitlistPosition { get; set; }
        public string? SelectedQuota { get; set; }
        public string TrainName { get; set; } = string.Empty;
        public string TrainNumber { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string TravelDate { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty;
        public string ArrivalTime { get; set; } = string.Empty;
        public string? TrainClass { get; set; }
        public List<PassengerDetails> Passengers { get; set; } = new List<PassengerDetails>();
    }

    public class PassengerDetails
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int? SeatNumber { get; set; }
    }

    public class CoachLayoutResponse
    {
        public List<CoachLayout> Coaches { get; set; } = new List<CoachLayout>();
    }

    public class CoachLayout
    {
        public int CoachNumber { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public List<SeatInfo> Seats { get; set; } = new List<SeatInfo>();
    }

    public class SeatInfo
    {
        public int SeatNumber { get; set; }
        public bool IsOccupied { get; set; }
    }

    public class AddCoachRequest
    {
        public List<CoachConfig> CoachConfigs { get; set; } = new List<CoachConfig>();
    }

    public class CoachConfig
    {
        public string Class { get; set; } = string.Empty;
        public string Quota { get; set; } = string.Empty;
        public int CoachCount { get; set; }
        public int SeatsPerCoach { get; set; }
        public decimal PriceMultiplier { get; set; }
    }

    public class AddCoachResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class WaitlistInfo
    {
        public int Position { get; set; }
        public int TotalWaiting { get; set; }
        public string EstimatedConfirmation { get; set; } = string.Empty;
    }
}