using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TransitHub.Data;
using TransitHub.Models;
using TransitHub.Repositories.Interfaces;
using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace TransitHub.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TransitHubDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed = false;

        // Repository instances
        private IGenericRepository<User>? _users;
        private IGenericRepository<Admin>? _admins;
        private IGenericRepository<Station>? _stations;
        private IGenericRepository<Airport>? _airports;
        private IGenericRepository<Train>? _trains;
        private IGenericRepository<Flight>? _flights;
        private IGenericRepository<TrainSchedule>? _trainSchedules;
        private IGenericRepository<FlightSchedule>? _flightSchedules;
        private IGenericRepository<Booking>? _bookings;
        private IGenericRepository<BookingPassenger>? _bookingPassengers;
        private IGenericRepository<Payment>? _payments;
        private IGenericRepository<WaitlistQueue>? _waitlistQueues;
        private IGenericRepository<Notification>? _notifications;
        private IGenericRepository<SystemLog>? _systemLogs;
        private IGenericRepository<BookingAudit>? _bookingAudits;
        private IGenericRepository<Cancellation>? _cancellations;
        private IGenericRepository<GmailVerificationToken>? _gmailVerificationTokens;
        private IGenericRepository<EmailVerificationOtp>? _emailVerificationOtps;
        
        // Lookup repositories
        private IGenericRepository<TrainQuotaType>? _trainQuotaTypes;
        private IGenericRepository<BookingStatusType>? _bookingStatusTypes;
        private IGenericRepository<PaymentMode>? _paymentModes;
        private IGenericRepository<TrainClass>? _trainClasses;
        private IGenericRepository<FlightClass>? _flightClasses;

        public UnitOfWork(TransitHubDbContext context)
        {
            _context = context;
        }

        // Repository Properties with Lazy Loading
        public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
        public IGenericRepository<Admin> Admins => _admins ??= new GenericRepository<Admin>(_context);
        public IGenericRepository<Station> Stations => _stations ??= new GenericRepository<Station>(_context);
        public IGenericRepository<Airport> Airports => _airports ??= new GenericRepository<Airport>(_context);
        public IGenericRepository<Train> Trains => _trains ??= new GenericRepository<Train>(_context);
        public IGenericRepository<Flight> Flights => _flights ??= new GenericRepository<Flight>(_context);
        public IGenericRepository<TrainSchedule> TrainSchedules => _trainSchedules ??= new GenericRepository<TrainSchedule>(_context);
        public IGenericRepository<FlightSchedule> FlightSchedules => _flightSchedules ??= new GenericRepository<FlightSchedule>(_context);
        public IGenericRepository<Booking> Bookings => _bookings ??= new GenericRepository<Booking>(_context);
        public IGenericRepository<BookingPassenger> BookingPassengers => _bookingPassengers ??= new GenericRepository<BookingPassenger>(_context);
        public IGenericRepository<Payment> Payments => _payments ??= new GenericRepository<Payment>(_context);
        public IGenericRepository<WaitlistQueue> WaitlistQueues => _waitlistQueues ??= new GenericRepository<WaitlistQueue>(_context);
        public IGenericRepository<Notification> Notifications => _notifications ??= new GenericRepository<Notification>(_context);
        public IGenericRepository<SystemLog> SystemLogs => _systemLogs ??= new GenericRepository<SystemLog>(_context);
        public IGenericRepository<BookingAudit> BookingAudits => _bookingAudits ??= new GenericRepository<BookingAudit>(_context);
        public IGenericRepository<Cancellation> Cancellations => _cancellations ??= new GenericRepository<Cancellation>(_context);
        public IGenericRepository<GmailVerificationToken> GmailVerificationTokens => _gmailVerificationTokens ??= new GenericRepository<GmailVerificationToken>(_context);
        public IGenericRepository<EmailVerificationOtp> EmailVerificationOtps => _emailVerificationOtps ??= new GenericRepository<EmailVerificationOtp>(_context);
        
        // Lookup Tables
        public IGenericRepository<TrainQuotaType> TrainQuotaTypes => _trainQuotaTypes ??= new GenericRepository<TrainQuotaType>(_context);
        public IGenericRepository<BookingStatusType> BookingStatusTypes => _bookingStatusTypes ??= new GenericRepository<BookingStatusType>(_context);
        public IGenericRepository<PaymentMode> PaymentModes => _paymentModes ??= new GenericRepository<PaymentMode>(_context);
        public IGenericRepository<TrainClass> TrainClasses => _trainClasses ??= new GenericRepository<TrainClass>(_context);
        public IGenericRepository<FlightClass> FlightClasses => _flightClasses ??= new GenericRepository<FlightClass>(_context);

        // Unit of Work Operations
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // Stored Procedure Execution at UnitOfWork Level
        public async Task<IEnumerable<TResult>> ExecuteStoredProcedureAsync<TResult>(string procedureName, params object[] parameters) where TResult : class, new()
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;
            
            // Add parameters (they should be SqlParameter objects)
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (param is SqlParameter sqlParam)
                    {
                        command.Parameters.Add(sqlParam);
                    }
                }
            }

            if (_context.Database.GetDbConnection().State != ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
            }
            
            try
            {
                using var reader = await command.ExecuteReaderAsync();
                var results = new List<TResult>();
                
                while (await reader.ReadAsync())
                {
                    var item = new TResult();
                    var properties = typeof(TResult).GetProperties();
                    
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var columnName = reader.GetName(i);
                        var property = properties.FirstOrDefault(p => 
                            string.Equals(p.Name, columnName, StringComparison.OrdinalIgnoreCase));
                        
                        if (property != null && reader[i] != DBNull.Value)
                        {
                            var value = reader[i];
                            
                            // Handle DateOnly conversion specially
                            if (property.PropertyType == typeof(DateOnly) || property.PropertyType == typeof(DateOnly?))
                            {
                                if (value is DateTime dateTime)
                                {
                                    value = DateOnly.FromDateTime(dateTime);
                                }
                                else if (value is string dateString && DateTime.TryParse(dateString, out var parsedDate))
                                {
                                    value = DateOnly.FromDateTime(parsedDate);
                                }
                            }
                            else if (property.PropertyType == typeof(TimeOnly) || property.PropertyType == typeof(TimeOnly?))
                            {
                                if (value is DateTime timeDateTime)
                                {
                                    value = TimeOnly.FromDateTime(timeDateTime);
                                }
                                else if (value is TimeSpan timeSpan)
                                {
                                    value = TimeOnly.FromTimeSpan(timeSpan);
                                }
                            }
                            else
                            {
                                value = Convert.ChangeType(value, property.PropertyType);
                            }
                            
                            property.SetValue(item, value);
                        }
                    }
                    
                    results.Add(item);
                }
                
                return results;
            }
            finally
            {
                if (_transaction == null) // Only close if not in transaction
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }
        }

        public async Task<int> ExecuteStoredProcedureNonQueryAsync(string procedureName, params object[] parameters)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;
            
            // Add parameters (they should be SqlParameter objects)
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (param is SqlParameter sqlParam)
                    {
                        command.Parameters.Add(sqlParam);
                    }
                }
            }

            if (_context.Database.GetDbConnection().State != ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
            }
            
            try
            {
                return await command.ExecuteNonQueryAsync();
            }
            finally
            {
                if (_transaction == null) // Only close if not in transaction
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }
        }

        public async Task<TResult?> ExecuteStoredProcedureScalarAsync<TResult>(string procedureName, params object[] parameters)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = procedureName;
            command.CommandType = CommandType.StoredProcedure;
            
            // Add parameters (they should be SqlParameter objects)
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    if (param is SqlParameter sqlParam)
                    {
                        command.Parameters.Add(sqlParam);
                    }
                }
            }

            if (_context.Database.GetDbConnection().State != ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
            }
            
            try
            {
                var result = await command.ExecuteScalarAsync();
                return result == null || result == DBNull.Value ? default(TResult) : (TResult)result;
            }
            finally
            {
                if (_transaction == null) // Only close if not in transaction
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }
        }

        // Dispose Pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}