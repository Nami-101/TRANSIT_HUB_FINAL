using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TransitHub.Models
{
    [Table("Trains")]
    public class Train : BaseEntity
    {
        [Key]
        public int TrainID { get; set; }

        [Required]
        [MaxLength(100)]
        public string TrainName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string TrainNumber { get; set; } = string.Empty;

        [Required]
        public int SourceStationID { get; set; }

        [Required]
        public int DestinationStationID { get; set; }

        // Navigation Properties
        [ForeignKey("SourceStationID")]
        public virtual Station SourceStation { get; set; } = null!;

        [ForeignKey("DestinationStationID")]
        public virtual Station DestinationStation { get; set; } = null!;

        public virtual ICollection<TrainSchedule> TrainSchedules { get; set; } = new List<TrainSchedule>();
        public virtual ICollection<Coach> Coaches { get; set; } = new List<Coach>();
    }
}