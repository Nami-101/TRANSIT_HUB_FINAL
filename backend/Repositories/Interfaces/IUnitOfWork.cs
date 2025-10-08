using TransitHub.Models;

namespace TransitHub.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Repository Properties
        IGenericRepository<User> Users { get; }
        IGenericRepository<Admin> Admins { get; }
        IGenericRepository<Station> Stations { get; }
        IGenericRepository<Airport> Airports { get; }
        IGenericRepository<Train> Trains { get; }
        IGenericRepository<Flight> Flights { get; }
        IGenericRepository<TrainSchedule> TrainSchedules { get; }
        IGenericRepository<FlightSchedule> FlightSchedules { get; }
        IGenericRepository<Booking> Bookings { get; }
        IGenericRepository<BookingPassenger> BookingPassengers { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<WaitlistQueue> WaitlistQueues { get; }
        IGenericRepository<Notification> Notifications { get; }
        IGenericRepository<SystemLog> SystemLogs { get; }
        IGenericRepository<BookingAudit> BookingAudits { get; }
        IGenericRepository<Cancellation> Cancellations { get; }
        IGenericRepository<GmailVerificationToken> GmailVerificationTokens { get; }
        IGenericRepository<EmailVerificationOtp> EmailVerificationOtps { get; }
        
        // Lookup Tables
        IGenericRepository<TrainQuotaType> TrainQuotaTypes { get; }
        IGenericRepository<BookingStatusType> BookingStatusTypes { get; }
        IGenericRepository<PaymentMode> PaymentModes { get; }
        IGenericRepository<TrainClass> TrainClasses { get; }
        IGenericRepository<FlightClass> FlightClasses { get; }
        
        // Unit of Work Operations
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        
        // Stored Procedure Execution
        Task<IEnumerable<TResult>> ExecuteStoredProcedureAsync<TResult>(string procedureName, params object[] parameters) where TResult : class, new();
        Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, params object[] parameters);
        Task<TResult?> ExecuteStoredProcedureScalarAsync<TResult>(string procedureName, params object[] parameters);
    }
}