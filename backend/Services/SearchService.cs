using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TransitHub.Models.DTOs;
using TransitHub.Repositories.Interfaces;
using TransitHub.Services.Interfaces;

namespace TransitHub.Services
{
    public class SearchService : ISearchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SearchService> _logger;

        public SearchService(IUnitOfWork unitOfWork, ILogger<SearchService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<TrainSearchResultDto>> SearchTrainsAsync(TrainSearchDto searchDto)
        {
            try
            {
                // Use stored procedure but group results to avoid quota duplicates
                var parameters = new object[]
                {
                    new SqlParameter("@SourceStation", searchDto.SourceStation),
                    new SqlParameter("@DestinationStation", searchDto.DestinationStation),
                    new SqlParameter("@TravelDate", searchDto.TravelDate.Date) { SqlDbType = SqlDbType.Date },
                    new SqlParameter("@TrainClass", (object?)searchDto.TrainClass ?? DBNull.Value),
                    new SqlParameter("@Quota", (object?)searchDto.Quota ?? DBNull.Value),
                    new SqlParameter("@PassengerCount", searchDto.PassengerCount)
                };

                var rawResults = await _unitOfWork.ExecuteStoredProcedureAsync<TrainSearchRawResultDto>(
                    "sp_SearchTrains", parameters);

                // Group by train and class to eliminate quota duplicates
                var groupedResults = rawResults
                    .GroupBy(r => new { r.TrainID, r.TrainClass })
                    .Select(g => new
                    {
                        TrainID = g.First().TrainID,
                        TrainNumber = g.First().TrainNumber,
                        TrainName = g.First().TrainName,
                        SourceStation = g.First().SourceStation,
                        SourceStationCode = g.First().SourceStationCode,
                        DestinationStation = g.First().DestinationStation,
                        DestinationStationCode = g.First().DestinationStationCode,
                        TravelDate = g.First().TravelDate,
                        DepartureTime = g.First().DepartureTime,
                        ArrivalTime = g.First().ArrivalTime,
                        JourneyTimeMinutes = g.First().JourneyTimeMinutes,
                        TrainClass = g.First().TrainClass,
                        ScheduleID = g.First().ScheduleID,
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
                        TrainID = group.TrainID,
                        TrainNumber = group.TrainNumber,
                        TrainName = group.TrainName,
                        SourceStation = group.SourceStation,
                        SourceStationCode = group.SourceStationCode,
                        DestinationStation = group.DestinationStation,
                        DestinationStationCode = group.DestinationStationCode,
                        TravelDate = group.TravelDate,
                        DepartureTime = group.DepartureTime,
                        ArrivalTime = group.ArrivalTime,
                        JourneyTimeMinutes = group.JourneyTimeMinutes,
                        TrainClass = group.TrainClass,
                        ScheduleID = group.ScheduleID,
                        TotalSeats = group.TotalSeats,
                        AvailableSeats = group.AvailableSeats,
                        Fare = group.Fare,
                        AvailabilityStatus = availabilityStatus,
                        AvailableOrWaitlistPosition = availableOrWaitlistPosition
                    });
                }
                
                results = results.OrderBy(r => r.DepartureTime).ThenBy(r => r.TrainClass).ToList();

                _logger.LogInformation("Train search completed. Found {Count} class results", results.Count);
                return results;
            }
            catch (SqlException ex) when (ex.Number >= 50010 && ex.Number <= 50015)
            {
                _logger.LogWarning("Train search validation error: {Message}", ex.Message);
                throw new ArgumentException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during train search");
                throw;
            }
        }

        public async Task<IEnumerable<FlightSearchResultDto>> SearchFlightsAsync(FlightSearchDto searchDto)
        {
            try
            {
                // Prepare parameters for stored procedure
                var parameters = new object[]
                {
                    new SqlParameter("@SourceAirportID", (object?)searchDto.SourceAirportID ?? DBNull.Value),
                    new SqlParameter("@DestinationAirportID", (object?)searchDto.DestinationAirportID ?? DBNull.Value),
                    new SqlParameter("@SourceAirportCode", (object?)searchDto.SourceAirportCode ?? DBNull.Value),
                    new SqlParameter("@DestinationAirportCode", (object?)searchDto.DestinationAirportCode ?? DBNull.Value),
                    new SqlParameter("@TravelDate", searchDto.TravelDate?.Date ?? DateTime.Today) { SqlDbType = SqlDbType.Date },
                    new SqlParameter("@FlightClassID", (object?)searchDto.FlightClassID ?? DBNull.Value),
                    new SqlParameter("@PassengerCount", searchDto.PassengerCount)
                };

                // Execute stored procedure
                var result = await _unitOfWork.ExecuteStoredProcedureAsync<FlightSearchResultDto>(
                    "sp_SearchFlights", parameters);

                _logger.LogInformation("Flight search completed. Found {Count} results", result.Count());
                return result;
            }
            catch (SqlException ex) when (ex.Number >= 50010 && ex.Number <= 50015)
            {
                _logger.LogWarning("Flight search validation error: {Message}", ex.Message);
                throw new ArgumentException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during flight search");
                throw;
            }
        }

        public async Task<IEnumerable<StationDto>> GetAllStationsAsync()
        {
            try
            {
                // Execute stored procedure
                var result = await _unitOfWork.ExecuteStoredProcedureAsync<StationDto>("sp_GetAllStations");

                _logger.LogInformation("Retrieved {Count} stations", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving stations");
                throw;
            }
        }

        public async Task<IEnumerable<AirportDto>> GetAllAirportsAsync()
        {
            try
            {
                // Execute stored procedure
                var result = await _unitOfWork.ExecuteStoredProcedureAsync<AirportDto>("sp_GetAllAirports");

                _logger.LogInformation("Retrieved {Count} airports", result.Count());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving airports");
                throw;
            }
        }

        private async Task<int> GetWaitlistCountAsync(int scheduleId, string trainClass)
        {
            try
            {
                var parameters = new object[]
                {
                    new SqlParameter("@TrainScheduleID", scheduleId),
                    new SqlParameter("@TrainClass", trainClass)
                };

                var result = await _unitOfWork.ExecuteStoredProcedureAsync<WaitlistPositionDto>(
                    "sp_GetNextWaitlistPosition", parameters);
                
                var count = result.FirstOrDefault()?.NextPosition ?? 0;
                _logger.LogInformation("Waitlist count for schedule {ScheduleId} class {TrainClass}: {Count}", scheduleId, trainClass, count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting waitlist count for schedule {ScheduleId} class {TrainClass}, defaulting to 0", scheduleId, trainClass);
                return 0; // Default to 0 if no waitlist exists
            }
        }

        public async Task<LookupDataDto> GetLookupDataAsync()
        {
            try
            {
                // This stored procedure returns multiple result sets
                // We'll need to execute it multiple times or modify the procedure to return JSON
                // For now, let's use individual calls to maintain simplicity

                var trainQuotaTypes = await _unitOfWork.ExecuteStoredProcedureAsync<TrainQuotaTypeDto>("sp_GetLookupData");
                var trainClasses = await _unitOfWork.TrainClasses.GetAllAsync();
                var flightClasses = await _unitOfWork.FlightClasses.GetAllAsync();
                var paymentModes = await _unitOfWork.PaymentModes.GetAllAsync();
                var bookingStatusTypes = await _unitOfWork.BookingStatusTypes.GetAllAsync();

                var lookupData = new LookupDataDto
                {
                    TrainQuotaTypes = trainQuotaTypes,
                    TrainClasses = trainClasses.Where(tc => tc.IsActive).Select(tc => new TrainClassDto
                    {
                        TrainClassID = tc.TrainClassID,
                        ClassName = tc.ClassName,
                        Description = tc.Description
                    }),
                    FlightClasses = flightClasses.Where(fc => fc.IsActive).Select(fc => new FlightClassDto
                    {
                        FlightClassID = fc.FlightClassID,
                        ClassName = fc.ClassName,
                        Description = fc.Description
                    }),
                    PaymentModes = paymentModes.Where(pm => pm.IsActive).Select(pm => new PaymentModeDto
                    {
                        PaymentModeID = pm.PaymentModeID,
                        ModeName = pm.ModeName,
                        Description = pm.Description
                    }),
                    BookingStatusTypes = bookingStatusTypes.Where(bst => bst.IsActive).Select(bst => new BookingStatusTypeDto
                    {
                        StatusID = bst.StatusID,
                        StatusName = bst.StatusName,
                        Description = bst.Description
                    })
                };

                _logger.LogInformation("Retrieved lookup data successfully");
                return lookupData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving lookup data");
                throw;
            }
        }
    }
}