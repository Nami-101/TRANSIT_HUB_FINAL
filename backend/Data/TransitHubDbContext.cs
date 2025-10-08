using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TransitHub.Models;

namespace TransitHub.Data
{
    public class TransitHubDbContext : IdentityDbContext<IdentityUser>
    {
        public TransitHubDbContext(DbContextOptions<TransitHubDbContext> options) : base(options)
        {
        }

        // User Management
        public new DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<GmailVerificationToken> GmailVerificationTokens { get; set; }
        public DbSet<EmailVerificationOtp> EmailVerificationOtps { get; set; }

        // Master Data
        public DbSet<Station> Stations { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<Train> Trains { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Coach> Coaches { get; set; }
        public DbSet<Seat> Seats { get; set; }

        // Lookup Tables
        public DbSet<TrainQuotaType> TrainQuotaTypes { get; set; }
        public DbSet<BookingStatusType> BookingStatusTypes { get; set; }
        public DbSet<PaymentMode> PaymentModes { get; set; }
        public DbSet<TrainClass> TrainClasses { get; set; }
        public DbSet<FlightClass> FlightClasses { get; set; }

        // Schedules
        public DbSet<TrainSchedule> TrainSchedules { get; set; }
        public DbSet<FlightSchedule> FlightSchedules { get; set; }

        // Bookings
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingPassenger> BookingPassengers { get; set; }
        public DbSet<TrainBooking> TrainBookings { get; set; }
        public DbSet<TrainBookingPassenger> TrainBookingPassengers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<WaitlistQueue> WaitlistQueues { get; set; }
        public DbSet<Cancellation> Cancellations { get; set; }

        // Audit & Logs
        public DbSet<BookingAudit> BookingAudits { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            ConfigureUserRelationships(modelBuilder);
            ConfigureStationRelationships(modelBuilder);
            ConfigureAirportRelationships(modelBuilder);
            ConfigureTrainRelationships(modelBuilder);
            ConfigureFlightRelationships(modelBuilder);
            ConfigureCoachRelationships(modelBuilder);
            ConfigureBookingRelationships(modelBuilder);
            ConfigureIndexes(modelBuilder);
            ConfigureConstraints(modelBuilder);
        }

        private void ConfigureUserRelationships(ModelBuilder modelBuilder)
        {
            // User unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();
        }

        private void ConfigureStationRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Station>()
                .HasIndex(s => s.StationCode)
                .IsUnique();
        }

        private void ConfigureAirportRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Airport>()
                .HasIndex(a => a.Code)
                .IsUnique();
        }

        private void ConfigureTrainRelationships(ModelBuilder modelBuilder)
        {
            // Train relationships
            modelBuilder.Entity<Train>()
                .HasOne(t => t.SourceStation)
                .WithMany(s => s.SourceTrains)
                .HasForeignKey(t => t.SourceStationID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Train>()
                .HasOne(t => t.DestinationStation)
                .WithMany(s => s.DestinationTrains)
                .HasForeignKey(t => t.DestinationStationID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Train>()
                .HasIndex(t => t.TrainNumber)
                .IsUnique();
        }

        private void ConfigureFlightRelationships(ModelBuilder modelBuilder)
        {
            // Flight relationships
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.SourceAirport)
                .WithMany(a => a.SourceFlights)
                .HasForeignKey(f => f.SourceAirportID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.DestinationAirport)
                .WithMany(a => a.DestinationFlights)
                .HasForeignKey(f => f.DestinationAirportID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasIndex(f => f.FlightNumber)
                .IsUnique();
        }

        private void ConfigureCoachRelationships(ModelBuilder modelBuilder)
        {
            // Coach relationships
            modelBuilder.Entity<Coach>()
                .HasOne(c => c.Train)
                .WithMany()
                .HasForeignKey(c => c.TrainID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Coach>()
                .HasOne(c => c.TrainClass)
                .WithMany()
                .HasForeignKey(c => c.TrainClassID)
                .OnDelete(DeleteBehavior.Restrict);

            // Seat relationships
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Coach)
                .WithMany(c => c.Seats)
                .HasForeignKey(s => s.CoachID)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraints
            modelBuilder.Entity<Coach>()
                .HasIndex(c => new { c.TrainID, c.CoachNumber })
                .IsUnique();

            modelBuilder.Entity<Seat>()
                .HasIndex(s => new { s.CoachID, s.SeatNumber })
                .IsUnique();
        }

        private void ConfigureBookingRelationships(ModelBuilder modelBuilder)
        {
            // Booking unique constraint
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingReference)
                .IsUnique();

            // TrainSchedule unique constraint
            modelBuilder.Entity<TrainSchedule>()
                .HasIndex(ts => new { ts.TrainID, ts.TravelDate, ts.QuotaTypeID, ts.TrainClassID })
                .IsUnique();

            // FlightSchedule unique constraint
            modelBuilder.Entity<FlightSchedule>()
                .HasIndex(fs => new { fs.FlightID, fs.TravelDate, fs.FlightClassID })
                .IsUnique();
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // Performance indexes
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.UserID);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingDate);

            modelBuilder.Entity<TrainSchedule>()
                .HasIndex(ts => ts.TravelDate);

            modelBuilder.Entity<FlightSchedule>()
                .HasIndex(fs => fs.TravelDate);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.BookingID);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserID, n.Status });

            modelBuilder.Entity<SystemLog>()
                .HasIndex(sl => sl.CreatedAt);

            modelBuilder.Entity<WaitlistQueue>()
                .HasIndex(wq => wq.Position);
        }

        private void ConfigureConstraints(ModelBuilder modelBuilder)
        {
            // Admin role constraint
            modelBuilder.Entity<Admin>()
                .Property(a => a.Role)
                .HasConversion<string>();

            // Booking type constraint
            modelBuilder.Entity<Booking>()
                .Property(b => b.BookingType)
                .HasConversion<string>();

            // Payment status constraint
            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<string>();

            // Gender constraint
            modelBuilder.Entity<BookingPassenger>()
                .Property(bp => bp.Gender)
                .HasConversion<string>();

            // Notification status constraint
            modelBuilder.Entity<Notification>()
                .Property(n => n.Status)
                .HasConversion<string>();

            // Log level constraint
            modelBuilder.Entity<SystemLog>()
                .Property(sl => sl.LogLevel)
                .HasConversion<string>();
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    // UpdatedBy should be set by the service layer
                }
            }
        }
    }
}