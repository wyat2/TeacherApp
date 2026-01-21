using System.ComponentModel.DataAnnotations;

namespace TeacherApp.ViewModels
{
    public class SetupWizardViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Batch Year is required")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Batch Year must be exactly 4 digits (e.g., 2025)")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Batch Year must be exactly 4 digits")]
        public string BatchYear { get; set; } = string.Empty;

        public bool CreateSampleData { get; set; } = false;

        public string AppName { get; set; } = "TeacherApp";

        public string SchoolName { get; set; } = string.Empty;
    }
}
