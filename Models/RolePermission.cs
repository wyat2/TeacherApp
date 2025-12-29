using System.ComponentModel.DataAnnotations;

namespace TeacherApp.Models
{
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RoleName { get; set; } = string.Empty;

        // Whether this role can delete student records
        public bool CanDeleteStudents { get; set; }

        // Whether this role can create/delete admin users
        public bool CanManageAdmins { get; set; }
        
        // Whether this role can delete Admin users
        public bool CanDeleteAdmins { get; set; }

        // Whether this role can delete Director users
        public bool CanDeleteDirectors { get; set; }

        // Whether this role can delete Tutor users
        public bool CanDeleteTutors { get; set; }
        
        // Whether this role can edit Admin users
        public bool CanEditAdmins { get; set; }

        // Whether this role can edit Director users
        public bool CanEditDirectors { get; set; }

        // Whether this role can edit Tutor users
        public bool CanEditTutors { get; set; }
    }
}
