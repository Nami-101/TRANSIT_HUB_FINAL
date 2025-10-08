using TransitHub.Models.DTOs;

namespace TransitHub.Services.Interfaces
{
    public interface IAdminService
    {
        // Dashboard
        Task<ApiResponse<DashboardMetricsDto>> GetDashboardMetricsAsync();

        // User Management
        Task<ApiResponse<List<AdminUserDto>>> GetAllUsersAsync();
        Task<ApiResponse> SoftDeleteUserAsync(string userId);

        // Train Management
        Task<ApiResponse<List<AdminTrainDto>>> GetAllTrainsAsync();
        Task<ApiResponse<AdminTrainDto>> GetTrainByIdAsync(int trainId);
        Task<ApiResponse<AdminTrainDto>> CreateTrainAsync(CreateTrainDto createTrainDto);
        Task<ApiResponse<AdminTrainDto>> UpdateTrainAsync(int trainId, UpdateTrainDto updateTrainDto);
        Task<ApiResponse> DeleteTrainAsync(int trainId);
        Task<ApiResponse> RemoveTrainAsync(int trainId);

        // Flight Management
        Task<ApiResponse<List<AdminFlightDto>>> GetAllFlightsAsync();
        Task<ApiResponse<AdminFlightDto>> GetFlightByIdAsync(int flightId);
        Task<ApiResponse<AdminFlightDto>> CreateFlightAsync(CreateFlightDto createFlightDto);
        Task<ApiResponse<AdminFlightDto>> UpdateFlightAsync(int flightId, UpdateFlightDto updateFlightDto);
        Task<ApiResponse> DeleteFlightAsync(int flightId);

        // Reports
        Task<ApiResponse<List<BookingReportDto>>> GetBookingReportsAsync();

        // Lookup Data (using DTOs from SearchDTOs.cs)
        Task<ApiResponse<List<StationDto>>> GetAllStationsAsync();
        Task<ApiResponse<List<AirportDto>>> GetAllAirportsAsync();

        // Station Management
        Task<ApiResponse<StationDto>> CreateStationAsync(CreateStationDto createStationDto);
        Task<ApiResponse<StationDto>> UpdateStationAsync(int stationId, UpdateStationDto updateStationDto);
        Task<ApiResponse> DeleteStationAsync(int stationId);

        // Coach Management
        Task<ApiResponse<List<CoachDto>>> GetTrainCoachesAsync(int trainId);
        Task<ApiResponse<CoachDto>> AddCoachToTrainAsync(int trainId, CreateCoachDto createCoachDto);
        Task<ApiResponse<CoachDto>> UpdateCoachAsync(int coachId, UpdateCoachDto updateCoachDto);
        Task<ApiResponse> DeleteCoachAsync(int coachId);
    }
}