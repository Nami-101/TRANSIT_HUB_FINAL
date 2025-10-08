using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitHub.Models
{
    [Table("Coaches")]
    public class Coach : BaseEntity
    {
        [Key]
        public int CoachID { get; set; }

        [Required]
        public int TrainID { get; set; }

        [Required]
        [MaxLength(10)]
        public string CoachNumber { get; set; } = string.Empty;

        [Required]
        public int TrainClassID { get; set; }

        [Required]
        public int TotalSeats { get; set; }

        [Required]
        public int AvailableSeats { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal BaseFare { get; set; }

        // Navigation Properties
        [ForeignKey("TrainID")]
        public virtual Train Train { get; set; } = null!;

        [ForeignKey("TrainClassID")]
        public virtual TrainClass TrainClass { get; set; } = null!;

        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    }
}