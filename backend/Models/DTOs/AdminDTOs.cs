namespace TransitHub.Models.DTOs
{
    // Dashboard Metrics
    public class DashboardMetricsDto
    {
        public int TotalUsers { get; set; }
        public int TotalTrains { get; set; }
        public int TotalFlights { get; set; }
        public int TotalBookings { get; set; }
        public int TotalStations { get; set; }
        public int TotalAirports { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveBookings { get; set; }
    }

    // User Management
    public class AdminUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    // Train Management
    public class AdminTrainDto
    {
        public int TrainId { get; set; }
        public string TrainNumber { get; set; } = string.Empty;
        public string TrainName { get; set; } = string.Empty;
        public int SourceStationId { get; set; }
        public string SourceStationName { get; set; } = string.Empty;
        public int DestinationStationId { get; set; }
        public string DestinationStationName { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty; // Simplified for admin view
        public string ArrivalTime { get; set; } = string.Empty; // Simplified for admin view
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTrainDto
    {
        public string TrainNumber { get; set; } = string.Empty;
        public string TrainName { get; set; } = string.Empty;
        public int SourceStationId { get; set; }
        public int DestinationStationId { get; set; }
        public string DepartureTime { get; set; } = string.Empty;
        public string ArrivalTime { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public List<int> SelectedClassIds { get; set; } = new();
    }

    public class UpdateTrainDto
    {
        public string TrainName { get; set; } = string.Empty;
        public int SourceStationId { get; set; }
        public int DestinationStationId { get; set; }
        public string DepartureTime { get; set; } = string.Empty;
        public string ArrivalTime { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; }
    }

    // Flight Management
    public class AdminFlightDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Airline { get; set; } = string.Empty;
        public int SourceAirportId { get; set; }
        public string SourceAirportName { get; set; } = string.Empty;
        public int DestinationAirportId { get; set; }
        public string DestinationAirportName { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty; // Simplified for admin view
        public string ArrivalTime { get; set; } = string.Empty; // Simplified for admin view
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateFlightDto
    {
        public string FlightNumber { get; set; } = string.Empty;
        public string Airline { get; set; } = string.Empty;
        public int SourceAirportId { get; set; }
        public int DestinationAirportId { get; set; }
        public string DepartureTime { get; set; } = string.Empty;
        public string ArrivalTime { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
    }

    public class UpdateFlightDto
    {
        public string Airline { get; set; } = string.Empty;
        public int SourceAirportId { get; set; }
        public int DestinationAirportId { get; set; }
        public string DepartureTime { get; set; } = string.Empty;
        public string ArrivalTime { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; }
    }

    // Booking Reports
    public class BookingReportDto
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string TransportType { get; set; } = string.Empty; // Train/Flight
        public string TransportNumber { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime TravelDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
    }

    // Station Management
    public class CreateStationDto
    {
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }

    public class UpdateStationDto
    {
        public string StationName { get; set; } = string.Empty;
        public string StationCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }

    // Coach Management
    public class CoachDto
    {
        public int CoachId { get; set; }
        public int TrainId { get; set; }
        public string CoachNumber { get; set; } = string.Empty;
        public int TrainClassId { get; set; }
        public string TrainClassName { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public decimal BaseFare { get; set; }
        public List<SeatDto> Seats { get; set; } = new();
    }

    public class CreateCoachDto
    {
        public string CoachNumber { get; set; } = string.Empty;
        public int TrainClassId { get; set; }
        public int TotalSeats { get; set; }
        public decimal BaseFare { get; set; }
    }

    public class UpdateCoachDto
    {
        public string CoachNumber { get; set; } = string.Empty;
        public int TrainClassId { get; set; }
        public int TotalSeats { get; set; }
        public decimal BaseFare { get; set; }
    }

    // Seat Management
    public class SeatDto
    {
        public int SeatId { get; set; }
        public int CoachId { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool IsWindowSeat { get; set; }
    }

    // Note: StationDto and AirportDto are already defined in SearchDTOs.cs
}