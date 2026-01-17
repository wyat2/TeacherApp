using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherApp.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public required string ContactType { get; set; }

        public string? Notes { get; set; }

        [Required]
        [ForeignKey("CreatedByUser")]
        public required string CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Student? Student { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }
    }
}
