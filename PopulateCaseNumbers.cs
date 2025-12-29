using Microsoft.EntityFrameworkCore;
using TeacherApp.Data;

namespace TeacherApp
{
    public class PopulateCaseNumbers
    {
        public static async Task Run(ApplicationDbContext context)
        {
            var studentsWithoutCaseNumber = await context.Students
                .Where(s => s.CaseNumber == null || s.CaseNumber == "")
                .ToListAsync();

            Console.WriteLine($"Found {studentsWithoutCaseNumber.Count} students without case numbers.");

            if (studentsWithoutCaseNumber.Count == 0)
            {
                Console.WriteLine("All students already have case numbers.");
                return;
            }

            // Find the highest existing case number
            var existingCaseNumbers = await context.Students
                .Where(s => s.CaseNumber != null && s.CaseNumber.StartsWith("CASE-"))
                .Select(s => s.CaseNumber)
                .ToListAsync();

            int maxNumber = 0;
            foreach (var caseNum in existingCaseNumbers)
            {
                if (caseNum != null && caseNum.Length > 5)
                {
                    var numPart = caseNum.Substring(5);
                    if (int.TryParse(numPart, out int num) && num > maxNumber)
                    {
                        maxNumber = num;
                    }
                }
            }

            Console.WriteLine($"Starting from case number: CASE-{(maxNumber + 1):D5}");

            // Assign case numbers
            int counter = maxNumber + 1;
            foreach (var student in studentsWithoutCaseNumber)
            {
                student.CaseNumber = $"CASE-{counter:D5}";
                Console.WriteLine($"Assigned {student.CaseNumber} to {student.FirstName} {student.LastName}");
                counter++;
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"\nSuccessfully populated case numbers for {studentsWithoutCaseNumber.Count} students.");
        }
    }
}
