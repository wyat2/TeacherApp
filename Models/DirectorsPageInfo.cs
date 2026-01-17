using System.ComponentModel.DataAnnotations;

namespace TeacherApp.Models
{
    public class DirectorsPageInfo
    {
        [Key]
        public int Id { get; set; }

        // School info
        public string? SchoolName { get; set; }
        public string? Address { get; set; }
        public string? SchoolPhone { get; set; }

        // Director info
        public string? DirectorName { get; set; }
        public string? DirectorEmail { get; set; }
        public string? DirectorPhone { get; set; }

        // Grant info
        public string? GrantType { get; set; }
        public string? PRAwardNumber { get; set; }
        public string? TypeOfInstitution { get; set; }
        public string? ProjectType { get; set; }
        public string? GPAScale { get; set; }
        public string? NameOfGranteeInstitution { get; set; }
        public decimal? GrantBudget { get; set; }
        public string? BatchYear { get; set; }
        public bool IsCurrent { get; set; }
    }
}
