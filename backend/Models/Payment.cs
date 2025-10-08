using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitHub.Models
{
    [Table("Payments")]
    public class Payment : BaseEntity
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        public int BookingID { get; set; }

        [Required]
        public int PaymentModeID { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty; // Pending, Success, Failed, Refunded

        [MaxLength(100)]
        public string? TransactionRef { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("BookingID")]
        public virtual Booking Booking { get; set; } = null!;

        [ForeignKey("PaymentModeID")]
        public virtual PaymentMode PaymentMode { get; set; } = null!;
    }

    [Table("WaitlistQueue")]
    public class WaitlistQueue : BaseEntity
    {
        [Key]
        public int WaitlistID { get; set; }

        [Required]
        public int BookingID { get; set; }

        [Required]
        public int TrainScheduleID { get; set; }

        [Required]
        public int Position { get; set; }

        [Required]
        public int Priority { get; set; } // 1 = Senior Citizen, 2 = Regular

        [MaxLength(10)]
        public string? TrainClass { get; set; } // 1A, 2S, SL, etc.

        [Required]
        public DateTime QueuedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ConfirmedAt { get; set; }

        // Navigation Properties
        [ForeignKey("BookingID")]
        public virtual Booking Booking { get; set; } = null!;

        [ForeignKey("TrainScheduleID")]
        public virtual TrainSchedule TrainSchedule { get; set; } = null!;
    }


}