using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitHub.Models
{
    [Table("TrainSchedules")]
    public class TrainSchedule : BaseEntity
    {
        [Key]
        public int ScheduleID { get; set; }

        [Required]
        public int TrainID { get; set; }

        [Required]
        public DateOnly TravelDate { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        public int QuotaTypeID { get; set; }

        [Required]
        public int TrainClassID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TotalSeats { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int AvailableSeats { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Fare { get; set; }

        // Navigation Properties
        [ForeignKey("TrainID")]
        public virtual Train Train { get; set; } = null!;

        [ForeignKey("QuotaTypeID")]
        public virtual TrainQuotaType QuotaType { get; set; } = null!;

        [ForeignKey("TrainClassID")]
        public virtual TrainClass TrainClass { get; set; } = null!;

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<WaitlistQueue> WaitlistQueues { get; set; } = new List<WaitlistQueue>();
    }

    [Table("FlightSchedules")]
    public class FlightSchedule : BaseEntity
    {
        [Key]
        public int ScheduleID { get; set; }

        [Required]
        public int FlightID { get; set; }

        [Required]
        public DateOnly TravelDate { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        public int FlightClassID { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TotalSeats { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int AvailableSeats { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Fare { get; set; }

        // Navigation Properties
        [ForeignKey("FlightID")]
        public virtual Flight Flight { get; set; } = null!;

        [ForeignKey("FlightClassID")]
        public virtual FlightClass FlightClass { get; set; } = null!;

        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}