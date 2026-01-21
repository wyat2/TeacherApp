using Microsoft.AspNetCore.Identity;
using TeacherApp.Models;
using TeacherApp.Data;
using Microsoft.EntityFrameworkCore;

namespace TeacherApp.Services
{
    public interface ISetupWizardService
    {
        Task<bool> IsFirstRunAsync();
        Task CompleteSetupAsync(string firstName, string lastName, string directorEmail, string directorPassword, string batchYear, bool createSampleData);
    }

    public class SetupWizardService : ISetupWizardService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly string _setupFlagPath;

        public SetupWizardService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _env = env;
            _setupFlagPath = Path.Combine(_env.ContentRootPath, ".setup-complete");
        }

        public async Task<bool> IsFirstRunAsync()
        {
            // Check if setup flag file exists
            if (File.Exists(_setupFlagPath))
                return false;

            try
            {
                // Also check if any admin users exist
                var adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole != null)
                {
                    var admins = await _userManager.GetUsersInRoleAsync("Admin");
                    if (admins.Count > 0)
                    {
                        // Mark setup as complete even if flag doesn't exist
                        await CreateSetupFlagAsync();
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                // If database tables don't exist yet, it's definitely first run
                return true;
            }

            return true;
        }

        public async Task CompleteSetupAsync(string firstName, string lastName, string directorEmail, string directorPassword, string batchYear, bool createSampleData)
        {
            // Ensure database is migrated first
            await _context.Database.MigrateAsync();
            
            // Create Director role if it doesn't exist
            if (!await _roleManager.RoleExistsAsync("Director"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Director"));
            }

            // Create Admin role if it doesn't exist (for backwards compatibility)
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create Tutor role if it doesn't exist
            if (!await _roleManager.RoleExistsAsync("Tutor"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Tutor"));
            }

            // Create director user
            var directorUser = await _userManager.FindByEmailAsync(directorEmail);
            if (directorUser == null)
            {
                directorUser = new ApplicationUser
                {
                    UserName = directorEmail,
                    Email = directorEmail,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName
                };
                var result = await _userManager.CreateAsync(directorUser, directorPassword);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create director user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Assign Director role
            if (!await _userManager.IsInRoleAsync(directorUser, "Director"))
            {
                await _userManager.AddToRoleAsync(directorUser, "Director");
            }

            // Create DirectorsPageInfo with the batch year
            var existingBatchYear = await _context.DirectorsPageInfos.FirstOrDefaultAsync(d => d.BatchYear == batchYear);
            if (existingBatchYear == null)
            {
                var directorsPageInfo = new DirectorsPageInfo
                {
                    BatchYear = batchYear,
                    IsCurrent = true,
                    DirectorEmail = directorEmail
                };
                _context.DirectorsPageInfos.Add(directorsPageInfo);
                await _context.SaveChangesAsync();
            }

            // Create sample data if requested
            if (createSampleData)
            {
                await CreateSampleDataAsync(batchYear, directorUser);
            }

            // Mark setup as complete
            await CreateSetupFlagAsync();
        }

        private async Task CreateSampleDataAsync(string batchYear, ApplicationUser createdByUser)
        {
            var random = new Random();
            
            // Sample names for admins and tutors
            var adminFirstNames = new[] { "Emily", "Michael", "Sarah", "David", "Jessica" };
            var adminLastNames = new[] { "Anderson", "Thompson", "Martinez", "Williams", "Taylor" };
            var tutorFirstNames = new[] { "Alex", "Morgan", "Jordan", "Casey", "Jamie" };
            var tutorLastNames = new[] { "Parker", "Brooks", "Rivera", "Foster", "Coleman" };
            
            // Create 5 Admin users
            for (int i = 1; i <= 5; i++)
            {
                var adminEmail = $"admin{i}@example.com";
                var existingAdmin = await _userManager.FindByEmailAsync(adminEmail);
                if (existingAdmin == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = adminFirstNames[i - 1],
                        LastName = adminLastNames[i - 1]
                    };
                    var result = await _userManager.CreateAsync(adminUser, "AdminPass123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            }

            // Create 5 Tutor users
            for (int i = 1; i <= 5; i++)
            {
                var tutorEmail = $"tutor{i}@example.com";
                var existingTutor = await _userManager.FindByEmailAsync(tutorEmail);
                if (existingTutor == null)
                {
                    var tutorUser = new ApplicationUser
                    {
                        UserName = tutorEmail,
                        Email = tutorEmail,
                        EmailConfirmed = true,
                        FirstName = tutorFirstNames[i - 1],
                        LastName = tutorLastNames[i - 1]
                    };
                    var result = await _userManager.CreateAsync(tutorUser, "TutorPass123!");
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(tutorUser, "Tutor");
                    }
                }
            }

            // Create Contact Type dropdown values
            var contactTypes = new[] { "Email", "Phone", "In Person", "Fax" };
            for (int i = 0; i < contactTypes.Length; i++)
            {
                var existingContactType = await _context.DropdownValues
                    .FirstOrDefaultAsync(d => d.FieldName == "ContactType" && d.Value == contactTypes[i]);
                
                if (existingContactType == null)
                {
                    _context.DropdownValues.Add(new DropdownValue
                    {
                        FieldName = "ContactType",
                        Value = contactTypes[i],
                        DisplayOrder = i + 1,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();

            // Create 200 sample students
            var firstNames = new[] { "JAMES", "MARY", "JOHN", "PATRICIA", "ROBERT", "JENNIFER", "MICHAEL", "LINDA", "WILLIAM", "BARBARA", 
                                    "DAVID", "ELIZABETH", "RICHARD", "SUSAN", "JOSEPH", "JESSICA", "THOMAS", "SARAH", "CHARLES", "KAREN",
                                    "DANIEL", "NANCY", "MATTHEW", "LISA", "ANTHONY", "BETTY", "MARK", "MARGARET", "DONALD", "SANDRA",
                                    "STEVEN", "ASHLEY", "PAUL", "KIMBERLY", "ANDREW", "EMILY", "JOSHUA", "DONNA", "KENNETH", "MICHELLE" };
            var lastNames = new[] { "SMITH", "JOHNSON", "WILLIAMS", "BROWN", "JONES", "GARCIA", "MILLER", "DAVIS", "RODRIGUEZ", "MARTINEZ",
                                   "HERNANDEZ", "LOPEZ", "GONZALEZ", "WILSON", "ANDERSON", "THOMAS", "TAYLOR", "MOORE", "JACKSON", "MARTIN",
                                   "LEE", "PEREZ", "THOMPSON", "WHITE", "HARRIS", "SANCHEZ", "CLARK", "RAMIREZ", "LEWIS", "ROBINSON" };
            var ethnicities = new[] { 
                "Ethnicity—Hispanic", 
                "Race—American Indian/Alaskan Native", 
                "Race—Asian", 
                "Race—Black or African American", 
                "Race—White", 
                "Race—Native Hawaiian or Other Pacific Islander",
                "Ethnicity—Hispanic, Race—Black or African American",
                "Race—Asian, Race—White"
            };
            var genders = new[] { "Male", "Female", "Non-Binary" };
            var eligibilities = new[] { "Low Income", "First Generation", "Disability", "Low Income, First Generation" };
            var academicStandings = new[] { "Freshman", "Sophomore", "Junior", "Senior", "Graduate Student" };
            var gradeLevels = new[] { "Freshman", "Sophomore", "Junior", "Senior" };
            var disabilities = new[] { "None", "Learning Disability", "Physical Disability", "Visual Impairment", "Hearing Impairment" };
            var academicNeeds = new[] { "None", "Math Tutoring", "Writing Support", "Study Skills", "Time Management" };
            var enrollmentStatuses = new[] { "Full-Time", "Part-Time", "Three-Quarter Time" };
            var participantStatuses = new[] { "Active", "Continuing", "New Participant", "Transferred" };
            var persistenceStatuses = new[] { "Continuing", "New Student", "Re-Entry", "Transfer Student" };
            var fieldsOfStudy = new[] { "Biology", "Business", "Computer Science", "Engineering", "English", "History", "Mathematics", 
                                       "Nursing", "Psychology", "Sociology", "Education", "Chemistry", "Physics", "Art", "Music" };
            var transferOptions = new[] { "No", "Yes - To Another Institution", "Yes - Within Same Institution" };

            for (int i = 1; i <= 200; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var middleInitial = ((char)('A' + random.Next(26))).ToString();
                var enrollmentDate = DateTime.UtcNow.AddMonths(-random.Next(6, 36));
                var cohortYear = batchYear;
                
                // Determine if student has graduated (10% chance)
                var hasGraduated = random.Next(100) < 10;
                var graduationDate = hasGraduated ? enrollmentDate.AddYears(random.Next(2, 5)) : (DateTime?)null;
                
                var student = new Student
                {
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleInitial = middleInitial,
                    Email = $"{firstName.ToLower()}.{lastName.ToLower()}{i}@student.edu",
                    DateOfBirth = DateTime.UtcNow.AddYears(-random.Next(18, 25)),
                    Gender = genders[random.Next(genders.Length)],
                    Ethnicity = ethnicities[random.Next(ethnicities.Length)],
                    Eligibility = eligibilities[random.Next(eligibilities.Length)],
                    StudentDisability = disabilities[random.Next(disabilities.Length)],
                    AcademicNeed = academicNeeds[random.Next(academicNeeds.Length)],
                    AcademicStanding = academicStandings[random.Next(academicStandings.Length)],
                    FirstEnrollmentDate = enrollmentDate,
                    DateOfFirstProjectService = enrollmentDate.AddDays(random.Next(1, 30)),
                    CumulativeGPA = Math.Round((decimal)(random.NextDouble() * 4.0), 2),
                    BatchYear = batchYear,
                    CaseNumber = $"CASE{i:D4}",
                    PRAwardNumber = $"P044A{random.Next(100000, 999999)}",
                    CollegeGradeLevelEntryIntoProject = gradeLevels[random.Next(gradeLevels.Length)],
                    EnrollmentStatusInAcademicYearFirstServed = enrollmentStatuses[random.Next(enrollmentStatuses.Length)],
                    StudentCohortYear = cohortYear,
                    ParticipantStatusDuringAcademicYearBeingReported = participantStatuses[random.Next(participantStatuses.Length)],
                    EnrollmentStatusAtEndOfAcademicYear = enrollmentStatuses[random.Next(enrollmentStatuses.Length)],
                    CollegeGradeLevelAtBeginningOfAcademicYear = gradeLevels[random.Next(gradeLevels.Length)],
                    CollegeGradeLevelAtEndOfAcademicYear = gradeLevels[random.Next(Math.Min(gradeLevels.Length, random.Next(1, gradeLevels.Length)))],
                    DateOfLastProjectService = DateTime.UtcNow.AddDays(-random.Next(1, 90)),
                    PersistenceStatusAtBeginningOfAcademicYear = persistenceStatuses[random.Next(persistenceStatuses.Length)],
                    AmountOfFinancialAidReceived = Math.Round((decimal)(random.NextDouble() * 15000 + 5000), 2),
                    AmountOfSSSGrantAidAwarded = Math.Round((decimal)(random.NextDouble() * 2000), 2),
                    Transfers = transferOptions[random.Next(transferOptions.Length)],
                    UndergraduateDegreeCertificateCompletedAtGranteeInstitution = hasGraduated ? "Yes" : "No",
                    DateOfUndergraduateDegreeCertificate = graduationDate,
                    FieldOfStudyDegreeCertificateEarned = hasGraduated ? fieldsOfStudy[random.Next(fieldsOfStudy.Length)] : null,
                    CreatedByUserId = createdByUser.Id
                };
                
                _context.Students.Add(student);
            }

            await _context.SaveChangesAsync();
        }

        private async Task CreateSetupFlagAsync()
        {
            await File.WriteAllTextAsync(_setupFlagPath, DateTime.UtcNow.ToString("o"));
        }
    }
}
