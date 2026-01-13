using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherApp.Models
{
    public class Test
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("StudentClass")]
        public int StudentClassId { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(50)]
        public string? Score { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("CreatedByUser")]
        public string? CreatedByUserId { get; set; }

        // Navigation properties
        public virtual StudentClass? StudentClass { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }
    }
}
