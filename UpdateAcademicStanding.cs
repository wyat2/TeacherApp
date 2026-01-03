using Microsoft.EntityFrameworkCore;
using TeacherApp.Data;

namespace TeacherApp
{
    public static class UpdateAcademicStanding
    {
        public static async Task Run(ApplicationDbContext context)
        {
            var students = await context.Students.ToListAsync();
            
            if (students.Count == 0)
            {
                Console.WriteLine("No students found to update.");
                return;
            }

            var academicStandings = new[] { "", "Freshman", "Sophomore", "Junior", "Senior", "Graduate Student" };
            var random = new Random();

            foreach (var student in students)
            {
                student.AcademicStanding = academicStandings[random.Next(academicStandings.Length)];
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"Updated {students.Count} student(s) with random Academic Standing values.");
        }
    }
}
