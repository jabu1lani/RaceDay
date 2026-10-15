using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation property
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}