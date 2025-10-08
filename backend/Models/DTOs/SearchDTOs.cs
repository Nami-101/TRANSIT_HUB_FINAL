using System.ComponentModel.DataAnnotations;

namespace TransitHub.Models.DTOs
{
    // Train Search DTOs
    public class TrainSearchDto
    {
        [Required]
        public string SourceStation { get; set; } = string.Empty;
        
        [Required]
        public string DestinationStation { get; set; } = string.Empty;
        
        [Required]
        public DateTime TravelDate { get; set; }
        
        public string? TrainClass { get; set; } // 2S, SL, 3A, FC, etc.
        
        public string? Quota { get; set; } // General, Tatkal, Ladies, Senior Citizen, etc.
        
        [Range(1, 6)]
        public int PassengerCount { get; set; } = 1;
    }

    public class TrainSearchResultDto
    {
        public int TrainID { get; set; }
        public string TrainName { get; set; } = string.Empty;
        public string TrainNumber { get; set; } = string.Empty;
        public string SourceStation { get; set; } = string.Empty;
        public string SourceStationCode { get; set; } = string.Empty;
        public string DestinationStation { get; set; } = string.Empty;
        public string DestinationStationCode { get; set; } = string.Empty;
        public DateOnly TravelDate { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int JourneyTimeMinutes { get; set; }
        public int ScheduleID { get; set; }
        public string QuotaName { get; set; } = string.Empty;
        public string TrainClass { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Fare { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;
        public int AvailableOrWaitlistPosition { get; set; }
    }

    public class TrainClassAvailabilityDto
    {
        public int ScheduleID { get; set; }
        public string TrainClass { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Fare { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;
        public int AvailableOrWaitlistPosition { get; set; }
    }

    // Internal DTO for raw stored procedure results
    public class TrainSearchRawResultDto
    {
        public int ScheduleID { get; set; }
        public int TrainID { get; set; }
        public string TrainName { get; set; } = string.Empty;
        public string TrainNumber { get; set; } = string.Empty;
        public string SourceStation { get; set; } = string.Empty;
        public string SourceStationCode { get; set; } = string.Empty;
        public string DestinationStation { get; set; } = string.Empty;
        public string DestinationStationCode { get; set; } = string.Empty;
        public DateOnly TravelDate { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int JourneyTimeMinutes { get; set; }
        public string QuotaName { get; set; } = string.Empty;
        public string TrainClass { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Fare { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;
        public int AvailableOrWaitlistPosition { get; set; }
    }

    // Flight Search DTOs
    public class FlightSearchDto
    {
        public int? SourceAirportID { get; set; }
        public int? DestinationAirportID { get; set; }
        public string? SourceAirportCode { get; set; }
        public string? DestinationAirportCode { get; set; }
        
        public DateTime? TravelDate { get; set; }
        
        public int? FlightClassID { get; set; }
        
        [Range(1, 6)]
        public int PassengerCount { get; set; } = 1;
    }

    public class FlightSearchResultDto
    {
        public int ScheduleID { get; set; }
        public int FlightID { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Airline { get; set; } = string.Empty;
        public string SourceAirport { get; set; } = string.Empty;
        public string SourceAirportCode { get; set; } = string.Empty;
        public string SourceCity { get; set; } = string.Empty;
        public string DestinationAirport { get; set; } = string.Empty;
        public string DestinationAirportCode { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public DateOnly TravelDate { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int FlightTimeMinutes { get; set; }
        public string FlightClass { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public decimal Fare { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty;
    }

    // Master Data DTOs
    public class StationDto
    {
        public int StationID { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
    }

    public class AirportDto
    {
        public int AirportID { get; set; }
        public string AirportName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    // Lookup Data DTOs
    public class LookupDataDto
    {
        public IEnumerable<TrainQuotaTypeDto> TrainQuotaTypes { get; set; } = new List<TrainQuotaTypeDto>();
        public IEnumerable<TrainClassDto> TrainClasses { get; set; } = new List<TrainClassDto>();
        public IEnumerable<FlightClassDto> FlightClasses { get; set; } = new List<FlightClassDto>();
        public IEnumerable<PaymentModeDto> PaymentModes { get; set; } = new List<PaymentModeDto>();
        public IEnumerable<BookingStatusTypeDto> BookingStatusTypes { get; set; } = new List<BookingStatusTypeDto>();
    }

    public class TrainQuotaTypeDto
    {
        public int QuotaTypeID { get; set; }
        public string QuotaName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class TrainClassDto
    {
        public int TrainClassID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class FlightClassDto
    {
        public int FlightClassID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class PaymentModeDto
    {
        public int PaymentModeID { get; set; }
        public string ModeName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class BookingStatusTypeDto
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class WaitlistPositionDto
    {
        public int NextPosition { get; set; }
    }
}