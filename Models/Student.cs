using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace TeacherApp.Models
{
    // Custom validation attribute for student names
    public class StudentNameAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success; // Let Required attribute handle empty values
            }

            var name = value.ToString()!;

            // Check length (0-9 characters)
            if (name.Length > 9)
            {
                return new ValidationResult("Name cannot exceed 9 characters.");
            }

            // Check for whitespace
            if (name.Contains(' '))
            {
                return new ValidationResult("Name cannot contain spaces.");
            }

            // Check for uppercase only and allowed special characters (. ' -)
            if (!Regex.IsMatch(name, @"^[A-Z.'\-]+$"))
            {
                return new ValidationResult("Name must be uppercase and can only contain letters, period (.), apostrophe ('), and dash (-).");
            }

            return ValidationResult.Success;
        }
    }

    public class Student
    {
        public int Id { get; set; }

        [Required]
        [StudentName]
        [StringLength(9, ErrorMessage = "First name cannot exceed 9 characters.")]
        public required string FirstName { get; set; }

        public string? MiddleInitial { get; set; }

        [Required]
        [StudentName]
        [StringLength(9, ErrorMessage = "Last name cannot exceed 9 characters.")]
        public required string LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        [ForeignKey("CreatedByUser")]
        public string? CreatedByUserId { get; set; }

        // New field: Ethnicity
        public string? Ethnicity { get; set; }

        // New field: Email
        public string? Email { get; set; }

        // New field: Case Number
        public string? CaseNumber { get; set; }

        // New field: PR/Award Number (from Grant Info)
        public string? PRAwardNumber { get; set; }

        // New field: Batch Year (from Grant Info)
        public string? BatchYear { get; set; }

        // Profile Picture
        public byte[]? ProfilePicture { get; set; }

        // Additional student information fields
        public string? Eligibility { get; set; }
        public string? StudentDisability { get; set; }
        public string? AcademicNeed { get; set; }
        public DateTime? FirstEnrollmentDate { get; set; }
        public DateTime? DateOfFirstProjectService { get; set; }
        public string? CollegeGradeLevelEntryIntoProject { get; set; }
        public string? EnrollmentStatusInAcademicYearFirstServed { get; set; }
        public string? StudentCohortYear { get; set; }
        public string? ParticipantStatusDuringAcademicYearBeingReported { get; set; }
        public string? EnrollmentStatusAtEndOfAcademicYear { get; set; }
        public string? AcademicStanding { get; set; }
        public decimal? CumulativeGPA { get; set; }
        public string? CollegeGradeLevelAtBeginningOfAcademicYear { get; set; }
        public string? CollegeGradeLevelAtEndOfAcademicYear { get; set; }
        public DateTime? DateOfLastProjectService { get; set; }
        public string? ReasonForWithdrawalOrNotReturning { get; set; }
        public string? Transfers { get; set; }
        public string? UndergraduateDegreeCertificateCompletedAtGranteeInstitution { get; set; }
        public DateTime? DateOfUndergraduateDegreeCertificate { get; set; }
        public string? FieldOfStudyDegreeCertificateEarned { get; set; }
        public string? PersistenceStatusAtBeginningOfAcademicYear { get; set; }
        public decimal? AmountOfFinancialAidReceived { get; set; }
        public decimal? AmountOfSSSGrantAidAwarded { get; set; }
        public string? ParticipantNameChange { get; set; }

        // Navigation property to the user who created the student
        public virtual ApplicationUser? CreatedByUser { get; set; }

        // Navigation property for student classes
        public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
        
        // Navigation property for contacts
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    }
}
