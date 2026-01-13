using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeacherApp.Models
{
    public class StudentClass
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [Required]
        public required string ClassName { get; set; }

        public string? Instructor { get; set; }

        public string? Schedule { get; set; }

        public string? Semester { get; set; }

        public string? Grade { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // Navigation property
        public virtual Student? Student { get; set; }
        public virtual ICollection<TutoringNote> TutoringNotes { get; set; } = new List<TutoringNote>();
        public virtual ICollection<Homework> Homework { get; set; } = new List<Homework>();
        public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
    }
}
