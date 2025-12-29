using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherApp.Models
{
    public class TutoringNote
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("StudentClass")]
        public int StudentClassId { get; set; }

        [Required]
        public required string Notes { get; set; }

        [Required]
        public DateTime NoteDate { get; set; }

        [Required]
        [ForeignKey("CreatedByUser")]
        public required string CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual StudentClass? StudentClass { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }
    }
}
