using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitHub.Models
{
    [Table("Cancellations")]
    public class Cancellation : BaseEntity
    {
        [Key]
        public int CancellationID { get; set; }

        [Required]
        public int BookingID { get; set; }

        [Required]
        public DateTime CancellationDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Reason { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal RefundAmount { get; set; }

        [Required]
        [MaxLength(20)]
        public string RefundStatus { get; set; } = "Pending"; // Pending, Processed, Failed

        [MaxLength(100)]
        public string? RefundTransactionRef { get; set; }

        public DateTime? RefundProcessedDate { get; set; }

        [MaxLength(500)]
        public string? RefundNotes { get; set; }

        // Navigation Properties
        [ForeignKey("BookingID")]
        public virtual Booking Booking { get; set; } = null!;
    }
}