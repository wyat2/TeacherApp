using System.ComponentModel.DataAnnotations;

namespace TeacherApp.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [MaxLength(256)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        [MaxLength(1000)]
        public string? Details { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(500)]
        public string? UserAgent { get; set; }
    }
}
