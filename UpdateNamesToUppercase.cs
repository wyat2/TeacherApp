using Microsoft.EntityFrameworkCore;
using TeacherApp.Data;

namespace TeacherApp
{
    public class UpdateNamesToUppercase
    {
        public static async Task Run(ApplicationDbContext context)
        {
            var students = await context.Students.ToListAsync();

            Console.WriteLine($"Found {students.Count} students to update.");

            if (students.Count == 0)
            {
                Console.WriteLine("No students found in the database.");
                return;
            }

            int updatedCount = 0;

            foreach (var student in students)
            {
                bool updated = false;

                // Update FirstName if not already uppercase
                if (student.FirstName != student.FirstName.ToUpper())
                {
                    var oldFirstName = student.FirstName;
                    student.FirstName = student.FirstName.ToUpper().Trim();
                    Console.WriteLine($"Updated FirstName: '{oldFirstName}' -> '{student.FirstName}'");
                    updated = true;
                }

                // Update LastName if not already uppercase
                if (student.LastName != student.LastName.ToUpper())
                {
                    var oldLastName = student.LastName;
                    student.LastName = student.LastName.ToUpper().Trim();
                    Console.WriteLine($"Updated LastName: '{oldLastName}' -> '{student.LastName}'");
                    updated = true;
                }

                // Update MiddleInitial if not already uppercase and not null
                if (!string.IsNullOrEmpty(student.MiddleInitial) && student.MiddleInitial != student.MiddleInitial.ToUpper())
                {
                    var oldMiddleInitial = student.MiddleInitial;
                    student.MiddleInitial = student.MiddleInitial.ToUpper().Trim();
                    Console.WriteLine($"Updated MiddleInitial: '{oldMiddleInitial}' -> '{student.MiddleInitial}'");
                    updated = true;
                }

                if (updated)
                {
                    updatedCount++;
                }
            }

            if (updatedCount > 0)
            {
                await context.SaveChangesAsync();
                Console.WriteLine($"\nSuccessfully updated {updatedCount} student record(s) to uppercase.");
            }
            else
            {
                Console.WriteLine("\nAll student names are already in uppercase. No updates needed.");
            }
        }
    }
}
