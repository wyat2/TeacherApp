using System.ComponentModel.DataAnnotations;

namespace TeacherApp.ViewModels
{
    public class SetupWizardViewModel
    {
        [Required(ErrorMessage = "Admin email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string AdminEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Admin password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the password")]
        [DataType(DataType.Password)]
        [Compare("AdminPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool CreateSampleData { get; set; } = false;

        public string AppName { get; set; } = "TeacherApp";

        public string SchoolName { get; set; } = string.Empty;
    }
}
