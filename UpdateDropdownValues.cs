using Microsoft.EntityFrameworkCore;
using TeacherApp.Data;

namespace TeacherApp;

public static class UpdateDropdownValues
{
    public static async Task UpdateExistingStudentData(ApplicationDbContext context)
    {
        Console.WriteLine("Updating existing student dropdown values...");
        
        var students = await context.Students.ToListAsync();
        var updateCount = 0;

        foreach (var student in students)
        {
            var updated = false;

            // Update Academic Need
            if (!string.IsNullOrEmpty(student.AcademicNeed) && !student.AcademicNeed.Contains(" - "))
            {
                student.AcademicNeed = student.AcademicNeed switch
                {
                    "0" => "0 - No response/Unknown",
                    "1" => "1 - Low high school grades",
                    "2" => "2 - Low admission test scores",
                    "5" => "5 - Predictive indicator",
                    "6" => "6 - Academic proficient tests",
                    "7" => "7 - Low college grades",
                    "8" => "8 - High school equivalency",
                    "9" => "9 - Failing grades",
                    "10" => "10 - Out of the academic pipeline for 5 or more years",
                    "11" => "11 - Other",
                    "12" => "12 - Limited English proficiency",
                    "13" => "13 - Lack of educational and/or career goals",
                    "14" => "14 - Lack of academic preparedness for college level course work",
                    "15" => "15 - Need for academic support to raise grade(s) in required course(s)/academic major",
                    _ => student.AcademicNeed
                };
                updated = true;
            }

            // Update Student Cohort Year
            if (!string.IsNullOrEmpty(student.StudentCohortYear) && !student.StudentCohortYear.Contains(" - "))
            {
                student.StudentCohortYear = student.StudentCohortYear switch
                {
                    "18" => "18 - 2016-17",
                    "19" => "19 - 2017-18",
                    "20" => "20 - 2018-19",
                    "21" => "21 - 2019-20",
                    "22" => "22 - 2020-21",
                    "23" => "23 - 2021-22",
                    "24" => "24 - 2022-23",
                    "99" => "99 - Not applicable",
                    _ => student.StudentCohortYear
                };
                updated = true;
            }

            // Update College Grade Level Entry Into Project
            if (!string.IsNullOrEmpty(student.CollegeGradeLevelEntryIntoProject) && !student.CollegeGradeLevelEntryIntoProject.Contains(" - "))
            {
                student.CollegeGradeLevelEntryIntoProject = student.CollegeGradeLevelEntryIntoProject switch
                {
                    "0" => "0 - No response/Unknown",
                    "1" => "1 - 1st yr., never attended",
                    "2" => "2 - 1st yr., attended before",
                    "3" => "3 - 2nd yr./sophomore",
                    "4" => "4 - 3rd yr./junior",
                    "5" => "5 - 4th yr./senior",
                    "6" => "6 - 5th yr./other undergraduate",
                    _ => student.CollegeGradeLevelEntryIntoProject
                };
                updated = true;
            }

            // Update College Grade Level At Beginning
            if (!string.IsNullOrEmpty(student.CollegeGradeLevelAtBeginningOfAcademicYear) && !student.CollegeGradeLevelAtBeginningOfAcademicYear.Contains(" - "))
            {
                student.CollegeGradeLevelAtBeginningOfAcademicYear = student.CollegeGradeLevelAtBeginningOfAcademicYear switch
                {
                    "0" => "0 - No response/Unknown",
                    "1" => "1 - 1st yr., never attended",
                    "2" => "2 - 1st yr., attended before",
                    "3" => "3 - 2nd yr./sophomore",
                    "4" => "4 - 3rd yr./junior",
                    "5" => "5 - 4th yr./senior",
                    "6" => "6 - 5th yr./other undergraduate",
                    _ => student.CollegeGradeLevelAtBeginningOfAcademicYear
                };
                updated = true;
            }

            // Update College Grade Level At End
            if (!string.IsNullOrEmpty(student.CollegeGradeLevelAtEndOfAcademicYear) && !student.CollegeGradeLevelAtEndOfAcademicYear.Contains(" - "))
            {
                student.CollegeGradeLevelAtEndOfAcademicYear = student.CollegeGradeLevelAtEndOfAcademicYear switch
                {
                    "0" => "0 - No response/Unknown",
                    "1" => "1 - 1st yr., never attended",
                    "2" => "2 - 1st yr., attended before",
                    "3" => "3 - 2nd yr./sophomore",
                    "4" => "4 - 3rd yr./junior",
                    "5" => "5 - 4th yr./senior",
                    "6" => "6 - 5th yr./other undergraduate",
                    _ => student.CollegeGradeLevelAtEndOfAcademicYear
                };
                updated = true;
            }

            // Update Eligibility
            if (!string.IsNullOrEmpty(student.Eligibility) && !student.Eligibility.Contains(" - "))
            {
                student.Eligibility = student.Eligibility switch
                {
                    "1" => "1 - Low-Income and First-Generation",
                    "2" => "2 - Low-Income only",
                    "3" => "3 - First-Generation only",
                    "4" => "4 - Disabled",
                    "5" => "5 - Disabled & Low-Income",
                    "0" => "0 - No response/Unknown",
                    _ => student.Eligibility
                };
                updated = true;
            }

            if (updated)
            {
                updateCount++;
                Console.WriteLine($"Updated student: {student.FirstName} {student.LastName} (ID: {student.Id})");
            }
        }

        if (updateCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"\n✅ Successfully updated {updateCount} student record(s)");
        }
        else
        {
            Console.WriteLine("No updates needed - all records already have full text values");
        }
    }
}
