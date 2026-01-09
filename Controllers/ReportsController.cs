using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using TeacherApp.Data;
using TeacherApp.Models;

namespace TeacherApp.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Get available batch years for the filter
            var batchYears = _context.DirectorsPageInfos
                .Where(d => !string.IsNullOrEmpty(d.BatchYear))
                .Select(d => d.BatchYear)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();
            
            ViewBag.AvailableBatchYears = batchYears;
            
            // Get current batch year from TempData
            ViewBag.CurrentBatchYear = TempData.Peek("CurrentBatchYear") as string;
            TempData.Keep("CurrentBatchYear");
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReport(
            string? academicStanding,
            string? eligibility,
            string? ethnicity,
            string? gender,
            string? batchYear,
            DateTime? enrollmentDateFrom,
            DateTime? enrollmentDateTo)
        {
            // Build query with filters
            var query = _context.Students
                .Include(s => s.CreatedByUser)
                .Include(s => s.StudentClasses)
                .AsQueryable();

            if (!string.IsNullOrEmpty(academicStanding))
            {
                query = query.Where(s => s.AcademicStanding == academicStanding);
            }

            if (!string.IsNullOrEmpty(eligibility))
            {
                query = query.Where(s => s.Eligibility == eligibility);
            }

            if (!string.IsNullOrEmpty(ethnicity))
            {
                query = query.Where(s => s.Ethnicity != null && s.Ethnicity.Contains(ethnicity));
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(s => s.Gender == gender);
            }

            if (!string.IsNullOrEmpty(batchYear))
            {
                query = query.Where(s => s.BatchYear == batchYear);
            }

            if (enrollmentDateFrom.HasValue)
            {
                query = query.Where(s => s.FirstEnrollmentDate >= enrollmentDateFrom.Value);
            }

            if (enrollmentDateTo.HasValue)
            {
                query = query.Where(s => s.FirstEnrollmentDate <= enrollmentDateTo.Value);
            }

            var students = await query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToListAsync();

            // Store filters in ViewBag for display
            ViewBag.AcademicStanding = academicStanding;
            ViewBag.Eligibility = eligibility;
            ViewBag.Ethnicity = ethnicity;
            ViewBag.Gender = gender;
            ViewBag.BatchYear = batchYear;
            ViewBag.EnrollmentDateFrom = enrollmentDateFrom;
            ViewBag.EnrollmentDateTo = enrollmentDateTo;

            // Return view with results
            return View("Results", students);
        }

        [HttpPost]
        public async Task<IActionResult> DownloadReport(
            string format,
            string? academicStanding,
            string? eligibility,
            string? ethnicity,
            string? gender,
            string? batchYear,
            DateTime? enrollmentDateFrom,
            DateTime? enrollmentDateTo)
        {
            // Build query with same filters
            var query = _context.Students
                .Include(s => s.CreatedByUser)
                .Include(s => s.StudentClasses)
                .AsQueryable();

            if (!string.IsNullOrEmpty(academicStanding))
            {
                query = query.Where(s => s.AcademicStanding == academicStanding);
            }

            if (!string.IsNullOrEmpty(eligibility))
            {
                query = query.Where(s => s.Eligibility == eligibility);
            }

            if (!string.IsNullOrEmpty(ethnicity))
            {
                query = query.Where(s => s.Ethnicity != null && s.Ethnicity.Contains(ethnicity));
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(s => s.Gender == gender);
            }

            if (!string.IsNullOrEmpty(batchYear))
            {
                query = query.Where(s => s.BatchYear == batchYear);
            }

            if (enrollmentDateFrom.HasValue)
            {
                query = query.Where(s => s.FirstEnrollmentDate >= enrollmentDateFrom.Value);
            }

            if (enrollmentDateTo.HasValue)
            {
                query = query.Where(s => s.FirstEnrollmentDate <= enrollmentDateTo.Value);
            }

            var students = await query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToListAsync();

            if (format == "csv")
            {
                return GenerateCSV(students);
            }
            else if (format == "pdf")
            {
                return GeneratePDF(students);
            }

            return BadRequest("Invalid format");
        }

        private IActionResult GenerateCSV(List<Student> students)
        {
            var csv = new StringBuilder();
            
            // Headers
            csv.AppendLine("Case Number,First Name,Middle Initial,Last Name,Email,Date of Birth,Gender,Ethnicity,Eligibility,First Enrollment Date,Cumulative GPA,Class Count,Created By");

            // Data rows
            foreach (var student in students)
            {
                csv.AppendLine($"\"{student.CaseNumber ?? ""}\",\"{student.FirstName}\",\"{student.MiddleInitial ?? ""}\",\"{student.LastName}\",\"{student.Email ?? ""}\",\"{student.DateOfBirth?.ToString("MM/dd/yyyy") ?? "00/00/0000"}\",\"{student.Gender ?? ""}\",\"{student.Ethnicity ?? ""}\",\"{student.Eligibility ?? ""}\",\"{student.FirstEnrollmentDate?.ToString("MM/dd/yyyy") ?? "00/00/0000"}\",\"{student.CumulativeGPA?.ToString() ?? ""}\",\"{student.StudentClasses?.Count ?? 0}\",\"{student.CreatedByUser?.Email ?? ""}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Students_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private IActionResult GeneratePDF(List<Student> students)
        {
            // Simple HTML to PDF approach
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; font-size: 10px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 6px; text-align: left; }");
            html.AppendLine("th { background-color: #667eea; color: white; font-weight: bold; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
            html.AppendLine("h1 { color: #333; text-align: center; }");
            html.AppendLine("</style>");
            html.AppendLine("</head><body>");
            html.AppendLine($"<h1>Students Report</h1>");
            html.AppendLine($"<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            html.AppendLine($"<p>Total Students: {students.Count}</p>");
            html.AppendLine("<table>");
            html.AppendLine("<thead><tr>");
            html.AppendLine("<th>Case Number</th>");
            html.AppendLine("<th>Name</th>");
            html.AppendLine("<th>Email</th>");
            html.AppendLine("<th>Gender</th>");
            html.AppendLine("<th>Ethnicity</th>");
            html.AppendLine("<th>Eligibility</th>");
            html.AppendLine("<th>GPA</th>");
            html.AppendLine("<th>Classes</th>");
            html.AppendLine("</tr></thead><tbody>");

            foreach (var student in students)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{student.CaseNumber ?? "-"}</td>");
                html.AppendLine($"<td>{student.FirstName} {student.MiddleInitial ?? ""} {student.LastName}</td>");
                html.AppendLine($"<td>{student.Email ?? "-"}</td>");
                html.AppendLine($"<td>{student.Gender ?? "-"}</td>");
                html.AppendLine($"<td>{student.Ethnicity ?? "-"}</td>");
                html.AppendLine($"<td>{student.Eligibility ?? "-"}</td>");
                html.AppendLine($"<td>{student.CumulativeGPA?.ToString("F2") ?? "-"}</td>");
                html.AppendLine($"<td>{student.StudentClasses?.Count ?? 0}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</tbody></table>");
            html.AppendLine("</body></html>");

            var bytes = Encoding.UTF8.GetBytes(html.ToString());
            return File(bytes, "text/html", $"Students_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAnnualPerformanceReport()
        {
            // Get currently selected batch year
            var currentBatchYear = TempData.Peek("CurrentBatchYear") as string;
            TempData.Keep("CurrentBatchYear"); // Keep TempData for subsequent requests
            
            // Get students filtered by current batch year
            var studentsQuery = _context.Students.AsQueryable();
            
            if (!string.IsNullOrEmpty(currentBatchYear) && currentBatchYear != "Creating New...")
            {
                studentsQuery = studentsQuery.Where(s => s.BatchYear == currentBatchYear);
            }
            
            var students = await studentsQuery
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            var csv = new StringBuilder();
            
            // Headers in numerical order (1-37, excluding 9-14 which will be individual ethnicity columns)
            csv.AppendLine("\"1-PR/Award Number\",\"2-Batch Year\",\"3-Case Number\",\"4-Last Name\",\"5-First Name\",\"6-Middle Initial\",\"7-Date of Birth\",\"8-Gender\"," +
                          "\"9-Ethnicity—Hispanic\",\"10-Race—American Indian/Alaskan Native\",\"11-Race—Asian\",\"12-Race—Black or African American\",\"13-Race—White\",\"14-Race—Native Hawaiian or Other Pacific Islander\"," +
                          "\"15a-Eligibility\",\"15b-Student Disability\",\"16-Academic Need\",\"17-First Enrollment Date\",\"18-Date Of First Project Service\"," +
                          "\"19-College Grade Level Entry Into Project\",\"20-Enrollment Status In Academic Year First Served\",\"21-Student Cohort Year\"," +
                          "\"22-Participant Status During Academic Year Being Reported\",\"23-Enrollment Status At End Of Academic Year\",\"24-Academic Standing\"," +
                          "\"25-Cumulative GPA\",\"26-College Grade Level At Beginning Of Academic Year\",\"27-College Grade Level At End Of Academic Year\"," +
                          "\"28-Date Of Last Project Service\",\"29-Reason For Withdrawal Or Not Returning\",\"30-Transfers\"," +
                          "\"31-Undergraduate Degree Certificate Completed At Grantee Institution\",\"32-Date Of Undergraduate Degree Certificate\"," +
                          "\"33-Field Of Study Degree Certificate Earned\",\"34-Persistence Status At Beginning Of Academic Year\"," +
                          "\"35-Amount Of Financial Aid Received\",\"36-Amount Of SSS Grant Aid Awarded\",\"37-Participant Name Change\"");

            // Data rows
            foreach (var student in students)
            {
                // Parse ethnicity for individual columns (9-14)
                var ethnicity = student.Ethnicity ?? "";
                var isHispanic = ethnicity.Contains("Ethnicity—Hispanic") ? "Yes" : "No";
                var isAmericanIndian = ethnicity.Contains("Race—American Indian/Alaskan Native") ? "Yes" : "No";
                var isAsian = ethnicity.Contains("Race—Asian") ? "Yes" : "No";
                var isBlack = ethnicity.Contains("Race—Black or African American") ? "Yes" : "No";
                var isWhite = ethnicity.Contains("Race—White") ? "Yes" : "No";
                var isHawaiian = ethnicity.Contains("Race—Native Hawaiian or Other Pacific Islander") ? "Yes" : "No";

                csv.AppendLine($"\"{EscapeCSV(student.PRAwardNumber)}\"," +
                              $"\"{EscapeCSV(student.BatchYear)}\"," +
                              $"\"{EscapeCSV(student.CaseNumber)}\"," +
                              $"\"{EscapeCSV(student.LastName)}\"," +
                              $"\"{EscapeCSV(student.FirstName)}\"," +
                              $"\"{EscapeCSV(student.MiddleInitial)}\"," +
                              $"\"{student.DateOfBirth?.ToString("MM/dd/yyyy") ?? "00/00/0000"}\"," +
                              $"\"{EscapeCSV(student.Gender)}\"," +
                              $"\"{isHispanic}\"," +
                              $"\"{isAmericanIndian}\"," +
                              $"\"{isAsian}\"," +
                              $"\"{isBlack}\"," +
                              $"\"{isWhite}\"," +
                              $"\"{isHawaiian}\"," +
                              $"\"{EscapeCSV(student.Eligibility)}\"," +
                              $"\"{EscapeCSV(student.StudentDisability)}\"," +
                              $"\"{EscapeCSV(student.AcademicNeed)}\"," +
                              $"\"{student.FirstEnrollmentDate?.ToString("MM/dd/yyyy") ?? "00/00/0000"}\"," +
                              $"\"{student.DateOfFirstProjectService?.ToString("MM/dd/yyyy") ?? "00/00/0000"}\"," +
                              $"\"{EscapeCSV(student.CollegeGradeLevelEntryIntoProject)}\"," +
                              $"\"{EscapeCSV(student.EnrollmentStatusInAcademicYearFirstServed)}\"," +
                              $"\"{EscapeCSV(student.StudentCohortYear)}\"," +
                              $"\"{EscapeCSV(student.ParticipantStatusDuringAcademicYearBeingReported)}\"," +
                              $"\"{EscapeCSV(student.EnrollmentStatusAtEndOfAcademicYear)}\"," +
                              $"\"{EscapeCSV(student.AcademicStanding)}\"," +
                              $"\"{student.CumulativeGPA?.ToString("F2") ?? ""}\"," +
                              $"\"{EscapeCSV(student.CollegeGradeLevelAtBeginningOfAcademicYear)}\"," +
                              $"\"{EscapeCSV(student.CollegeGradeLevelAtEndOfAcademicYear)}\"," +
                              $"\"{student.DateOfLastProjectService?.ToString("MM/dd/yyyy") ?? "00/00/0000"}\"," +
                              $"\"{EscapeCSV(student.ReasonForWithdrawalOrNotReturning)}\"," +
                              $"\"{EscapeCSV(student.Transfers)}\"," +
                              $"\"{EscapeCSV(student.UndergraduateDegreeCertificateCompletedAtGranteeInstitution)}\"," +
                              $"\"{student.DateOfUndergraduateDegreeCertificate?.ToString("MM/dd/yyyy") ?? "00/00/0000"}\"," +
                              $"\"{EscapeCSV(student.FieldOfStudyDegreeCertificateEarned)}\"," +
                              $"\"{EscapeCSV(student.PersistenceStatusAtBeginningOfAcademicYear)}\"," +
                              $"\"{student.AmountOfFinancialAidReceived?.ToString("F2") ?? ""}\"," +
                              $"\"{student.AmountOfSSSGrantAidAwarded?.ToString("F2") ?? ""}\"," +
                              $"\"{EscapeCSV(student.ParticipantNameChange)}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Annual_Performance_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private string EscapeCSV(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            
            // Escape quotes by doubling them
            return value.Replace("\"", "\"\"");
        }
    }
}
