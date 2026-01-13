using Microsoft.EntityFrameworkCore;
using TeacherApp.Data;
using TeacherApp.Models;

namespace TeacherApp
{
    public class PopulateTestData
    {
        private static readonly Random random = new Random();
        
        private static readonly string[] firstNames = { "JAMES", "MARY", "JOHN", "PATRICIA", "ROBERT", "JENNIFER", "MICHAEL", "LINDA", "WILLIAM", "BARBARA",
            "DAVID", "ELIZABETH", "RICHARD", "SUSAN", "JOSEPH", "JESSICA", "THOMAS", "SARAH", "CHARLES", "KAREN",
            "DANIEL", "NANCY", "MATTHEW", "LISA", "ANTHONY", "BETTY", "MARK", "MARGARET", "DONALD", "SANDRA",
            "STEVEN", "ASHLEY", "PAUL", "KIMBERLY", "ANDREW", "EMILY", "JOSHUA", "DONNA", "KENNETH", "MICHELLE",
            "KEVIN", "DOROTHY", "BRIAN", "CAROL", "GEORGE", "AMANDA", "EDWARD", "MELISSA", "RONALD", "DEBORAH" };
        
        private static readonly string[] lastNames = { "SMITH", "JOHNSON", "WILLIAMS", "BROWN", "JONES", "GARCIA", "MILLER", "DAVIS", "RODRIGUEZ", "MARTINEZ",
            "HERNANDEZ", "LOPEZ", "GONZALEZ", "WILSON", "ANDERSON", "THOMAS", "TAYLOR", "MOORE", "JACKSON", "MARTIN",
            "LEE", "PEREZ", "THOMPSON", "WHITE", "HARRIS", "SANCHEZ", "CLARK", "RAMIREZ", "LEWIS", "ROBINSON",
            "WALKER", "YOUNG", "ALLEN", "KING", "WRIGHT", "SCOTT", "TORRES", "NGUYEN", "HILL", "FLORES",
            "GREEN", "ADAMS", "NELSON", "BAKER", "HALL", "RIVERA", "CAMPBELL", "MITCHELL", "CARTER", "ROBERTS" };
        
        private static readonly string[] ethnicities = { "Hispanic/Latino", "White", "Black/African American", "Asian", "Native American", "Pacific Islander", "Two or more races" };
        
        private static readonly string[] genders = { "Male", "Female", "Non-binary" };
        
        private static readonly string[] classNames = { "MATH 101", "ENG 102", "HIST 201", "BIO 150", "CHEM 120", "PHYS 180", 
            "PSYCH 100", "SOC 110", "ART 105", "MUS 101", "CS 200", "ECON 101", "PHIL 150", "SPAN 101", "FREN 102" };
        
        private static readonly string[] instructors = { "Dr. Anderson", "Prof. Baker", "Dr. Carter", "Prof. Davis", "Dr. Evans",
            "Prof. Fisher", "Dr. Garcia", "Prof. Harris", "Dr. Jackson", "Prof. Klein" };
        
        private static readonly string[] semesters = { "Fall 2023", "Spring 2024", "Summer 2024", "Fall 2024" };
        
        private static readonly string[] schedules = { "MWF 9:00-10:00", "TR 10:30-12:00", "MWF 1:00-2:00", "TR 2:30-4:00", "MW 6:00-7:30" };
        
        private static readonly string[] grades = { "A", "A-", "B+", "B", "B-", "C+", "C", "C-" };
        
        private static readonly string[] academicStandings = { "Freshman", "Sophomore", "Junior", "Senior", "Graduate Student" };
        
        private static readonly string[] eligibilities = { "1 - Low-Income and First-Generation", "2 - Low-Income only", "3 - First-Generation only", 
            "4 - Disabled", "5 - Disabled & Low-Income", "0 - No response/Unknown" };
        
        private static readonly string[] studentDisabilities = { "Yes", "No" };
        
        private static readonly string[] academicNeeds = { "0 - No response/Unknown", "1 - Low high school grades", "2 - Low admission test scores",
            "5 - Predictive indicator", "6 - Academic proficient tests", "7 - Low college grades", "8 - High school equivalency",
            "9 - Failing grades", "10 - Out of the academic pipeline for 5 or more years", "11 - Other", "12 - Limited English proficiency",
            "13 - Lack of educational and/or career goals", "14 - Lack of academic preparedness for college level course work",
            "15 - Need for academic support to raise grade(s) in required course(s)/academic major" };
        
        private static readonly string[] cohortYears = { "18 - 2016-17", "19 - 2017-18", "20 - 2018-19", "21 - 2019-20", 
            "22 - 2020-21", "23 - 2021-22", "24 - 2022-23", "99 - Not applicable" };
        
        private static readonly string[] gradeLevels = { "0 - No response/Unknown", "1 - 1st yr., never attended", "2 - 1st yr., attended before",
            "3 - 2nd yr./sophomore", "4 - 3rd yr./junior", "5 - 4th yr./senior", "6 - 5th yr./other undergraduate" };
        
        private static readonly string[] enrollmentStatuses = { "Full-time", "Part-time", "Three-quarter time", "Half-time", "Less than half-time" };
        
        private static readonly string[] participantStatuses = { "Active", "Inactive", "Completed", "Withdrawn", "On Leave" };
        
        private static readonly string[] persistenceStatuses = { "New to institution", "Continuing/Re-enrolled", "Transferred from another institution", 
            "Graduated/Completed", "Did not re-enroll" };
        
        private static readonly string[] fieldsOfStudy = { "Business", "Engineering", "Education", "Health Sciences", "Liberal Arts", 
            "Computer Science", "Biology", "Psychology", "Nursing", "Communications", "Criminal Justice", "Social Work" };
        
        public static async Task PopulateAsync(ApplicationDbContext context)
        {
            Console.WriteLine("Starting database population...");
            
            // Get an admin user to assign as creator
            var adminUser = await context.Users.FirstOrDefaultAsync();
            if (adminUser == null)
            {
                Console.WriteLine("No users found in database. Please create a user first.");
                return;
            }
            
            // Create 100 students
            for (int i = 1; i <= 100; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                
                var student = new Student
                {
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleInitial = ((char)('A' + random.Next(26))).ToString(),
                    DateOfBirth = DateTime.Now.AddYears(-random.Next(18, 25)).AddDays(-random.Next(365)),
                    Gender = genders[random.Next(genders.Length)],
                    Ethnicity = ethnicities[random.Next(ethnicities.Length)],
                    Email = $"{firstName.ToLower()}.{lastName.ToLower()}@university.edu",
                    CaseNumber = $"2024-{i:D4}",
                    PRAwardNumber = "P042A200001",
                    BatchYear = "2024",
                    CreatedByUserId = adminUser.Id,
                    
                    // Academic Information
                    AcademicStanding = academicStandings[random.Next(academicStandings.Length)],
                    CumulativeGPA = (decimal)Math.Round(random.NextDouble() * 4.0, 2),
                    CollegeGradeLevelAtBeginningOfAcademicYear = gradeLevels[random.Next(1, gradeLevels.Length)],
                    CollegeGradeLevelAtEndOfAcademicYear = gradeLevels[random.Next(1, gradeLevels.Length)],
                    CollegeGradeLevelEntryIntoProject = gradeLevels[random.Next(1, gradeLevels.Length)],
                    PersistenceStatusAtBeginningOfAcademicYear = persistenceStatuses[random.Next(persistenceStatuses.Length)],
                    
                    // Eligibility and Needs
                    Eligibility = eligibilities[random.Next(eligibilities.Length)],
                    StudentDisability = studentDisabilities[random.Next(studentDisabilities.Length)],
                    AcademicNeed = academicNeeds[random.Next(academicNeeds.Length)],
                    
                    // Enrollment Information
                    FirstEnrollmentDate = DateTime.Now.AddYears(-random.Next(1, 4)).AddDays(-random.Next(365)),
                    DateOfFirstProjectService = DateTime.Now.AddMonths(-random.Next(6, 18)),
                    DateOfLastProjectService = DateTime.Now.AddDays(-random.Next(1, 30)),
                    EnrollmentStatusInAcademicYearFirstServed = enrollmentStatuses[random.Next(enrollmentStatuses.Length)],
                    EnrollmentStatusAtEndOfAcademicYear = enrollmentStatuses[random.Next(enrollmentStatuses.Length)],
                    StudentCohortYear = cohortYears[random.Next(cohortYears.Length)],
                    ParticipantStatusDuringAcademicYearBeingReported = participantStatuses[random.Next(participantStatuses.Length)],
                    
                    // Financial Information
                    AmountOfFinancialAidReceived = (decimal)Math.Round(5000 + random.NextDouble() * 15000, 2),
                    AmountOfSSSGrantAidAwarded = (decimal)Math.Round(500 + random.NextDouble() * 2500, 2),
                    
                    // Completion Information (randomly fill for some students)
                    Transfers = random.Next(3) == 0 ? "Yes" : "No",
                    UndergraduateDegreeCertificateCompletedAtGranteeInstitution = random.Next(5) == 0 ? "Bachelor of Science" : null,
                    DateOfUndergraduateDegreeCertificate = random.Next(5) == 0 ? DateTime.Now.AddMonths(-random.Next(1, 12)) : null,
                    FieldOfStudyDegreeCertificateEarned = random.Next(5) == 0 ? fieldsOfStudy[random.Next(fieldsOfStudy.Length)] : null,
                    ReasonForWithdrawalOrNotReturning = random.Next(10) == 0 ? "Personal reasons" : null,
                    ParticipantNameChange = random.Next(10) == 0 ? "Name changed from previous record" : null
                };
                
                context.Students.Add(student);
                await context.SaveChangesAsync();
                
                // Add 5 classes for each student
                for (int j = 0; j < 5; j++)
                {
                    var className = classNames[random.Next(classNames.Length)];
                    var startDate = DateTime.Now.AddMonths(-random.Next(2, 6));
                    
                    var studentClass = new StudentClass
                    {
                        StudentId = student.Id,
                        ClassName = className,
                        Instructor = instructors[random.Next(instructors.Length)],
                        Schedule = schedules[random.Next(schedules.Length)],
                        Semester = semesters[random.Next(semesters.Length)],
                        Grade = grades[random.Next(grades.Length)],
                        StartDate = startDate,
                        EndDate = startDate.AddMonths(4)
                    };
                    
                    context.StudentClasses.Add(studentClass);
                    await context.SaveChangesAsync();
                    
                    // Add 2-4 tutoring notes per class
                    int noteCount = random.Next(2, 5);
                    for (int k = 0; k < noteCount; k++)
                    {
                        var tutoringNote = new TutoringNote
                        {
                            StudentClassId = studentClass.Id,
                            NoteDate = DateTime.Now.AddDays(-random.Next(1, 60)),
                            Notes = $"Session {k + 1}: Student showed {(random.Next(2) == 0 ? "good" : "excellent")} progress on {className}. " +
                                   $"Covered topics: {(random.Next(2) == 0 ? "homework review" : "exam preparation")}. " +
                                   $"{(random.Next(2) == 0 ? "Needs more practice." : "Ready for next topic.")}",
                            CreatedByUserId = adminUser.Id,
                            CreatedAt = DateTime.Now.AddDays(-random.Next(1, 60))
                        };
                        
                        context.TutoringNotes.Add(tutoringNote);
                    }
                    
                    // Add 3-5 homework assignments per class
                    int homeworkCount = random.Next(3, 6);
                    for (int h = 0; h < homeworkCount; h++)
                    {
                        var homework = new Homework
                        {
                            StudentClassId = studentClass.Id,
                            Name = $"Assignment {h + 1}",
                            Date = DateTime.Now.AddDays(-random.Next(1, 90)),
                            Score = $"{random.Next(70, 101)}/100",
                            CreatedByUserId = adminUser.Id,
                            CreatedAt = DateTime.Now.AddDays(-random.Next(1, 90))
                        };
                        
                        context.Homework.Add(homework);
                    }
                    
                    // Add 2-3 tests per class
                    int testCount = random.Next(2, 4);
                    for (int t = 0; t < testCount; t++)
                    {
                        var test = new Test
                        {
                            StudentClassId = studentClass.Id,
                            Name = $"{(t == 0 ? "Midterm" : t == 1 ? "Final" : "Quiz " + (t - 1))}",
                            Date = DateTime.Now.AddDays(-random.Next(1, 90)),
                            Score = $"{random.Next(65, 101)}/100",
                            CreatedByUserId = adminUser.Id,
                            CreatedAt = DateTime.Now.AddDays(-random.Next(1, 90))
                        };
                        
                        context.Tests.Add(test);
                    }
                    
                    await context.SaveChangesAsync();
                }
                
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Created {i} students with classes, notes, homework, and tests...");
                }
            }
            
            Console.WriteLine("Database population completed successfully!");
            Console.WriteLine("Created 100 students with 5 classes each, plus tutoring notes, homework, and tests.");
        }
    }
}
