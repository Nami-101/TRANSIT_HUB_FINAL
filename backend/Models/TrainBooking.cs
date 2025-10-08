using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitHub.Models
{
    [Table("TrainBookings")]
    public class TrainBooking : BaseEntity
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int TrainId { get; set; }

        [Required]
        public int ScheduleId { get; set; }

        [Required]
        [MaxLength(20)]
        public string BookingReference { get; set; } = string.Empty;

        [Required]
        public DateTime TravelDate { get; set; }

        [Required]
        public int PassengerCount { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty; // Confirmed, Cancelled, Waitlisted

        [MaxLength(50)]
        public string? SelectedQuota { get; set; }

        public bool AutoAssignSeats { get; set; } = true;

        [Required]
        public DateTime BookingDate { get; set; }

        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Train Train { get; set; } = null!;
        public virtual TrainSchedule Schedule { get; set; } = null!;
        public virtual ICollection<TrainBookingPassenger> Passengers { get; set; } = new List<TrainBookingPassenger>();
    }

    [Table("TrainBookingPassengers")]
    public class TrainBookingPassenger : BaseEntity
    {
        [Key]
        public int PassengerId { get; set; }

        [Required]
        public int BookingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 120)]
        public int Age { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? SeatNumber { get; set; }

        [MaxLength(10)]
        public string? CoachNumber { get; set; }

        [MaxLength(20)]
        public string? SeatPreference { get; set; }

        public bool IsSeniorCitizen { get; set; }

        // Navigation Properties
        public virtual TrainBooking Booking { get; set; } = null!;
    }
}