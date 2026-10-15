using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal EntryFee { get; set; }

        [Required]
        public int MaxParticipants { get; set; }

        [Required]
        public int EventId { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event? Event { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}