using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitHub.Models
{
    [Table("Seats")]
    public class Seat : BaseEntity
    {
        [Key]
        public int SeatID { get; set; }

        [Required]
        public int CoachID { get; set; }

        [Required]
        [MaxLength(10)]
        public string SeatNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string SeatType { get; set; } = string.Empty; // Upper, Lower, Middle, Side Upper, Side Lower

        [Required]
        public bool IsAvailable { get; set; } = true;

        [Required]
        public bool IsWindowSeat { get; set; } = false;

        // Navigation Properties
        [ForeignKey("CoachID")]
        public virtual Coach Coach { get; set; } = null!;
    }
}