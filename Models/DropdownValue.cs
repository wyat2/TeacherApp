using System.ComponentModel.DataAnnotations;

namespace TeacherApp.Models
{
    public class DropdownValue
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FieldName { get; set; }

        [Required]
        [MaxLength(255)]
        public required string Value { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
