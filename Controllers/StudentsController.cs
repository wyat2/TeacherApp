using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using TeacherApp.Data;
using TeacherApp.Models;
using TeacherApp.Services;

namespace TeacherApp.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<string> GenerateUniqueCaseNumberAsync()
        {
            // Find the highest existing case number
            var maxCaseNumber = await _context.Students
                .Where(s => s.CaseNumber != null && s.CaseNumber.StartsWith("CASE-"))
                .Select(s => s.CaseNumber)
                .ToListAsync();

            int maxNumber = 0;
            foreach (var caseNum in maxCaseNumber)
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

            // Generate next case number
            return $"CASE-{(maxNumber + 1):D5}";
        }

        // helper to check whether current user has permission to delete students
        private bool UserHasDeleteStudentPermission()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return false;
            var roles = _context.RolePermissions.Where(p => p.CanDeleteStudents).Select(p => p.RoleName).ToList();
            foreach (var r in roles)
            {
                if (User.IsInRole(r)) return true;
            }
            return false;
        }

        public async Task<IActionResult> Index(string? q, string? className)
        {
            var studentsQuery = _context.Students.Include(s => s.CreatedByUser).AsQueryable();
            
            // Filter by current batch year
            var currentBatchYear = TempData.Peek("CurrentBatchYear") as string;
            TempData.Keep("CurrentBatchYear"); // Keep TempData for subsequent requests
            if (!string.IsNullOrEmpty(currentBatchYear) && currentBatchYear != "Creating New...")
            {
                studentsQuery = studentsQuery.Where(s => s.BatchYear == currentBatchYear);
            }
            
            if (!string.IsNullOrWhiteSpace(q))
            {
                var lower = q.ToLower();
                studentsQuery = studentsQuery.Where(s => ((s.FirstName ?? string.Empty).ToLower().Contains(lower))
                                                    || ((s.LastName ?? string.Empty).ToLower().Contains(lower))
                                                    || ((s.Email ?? string.Empty).ToLower().Contains(lower)));
            }
            if (!string.IsNullOrWhiteSpace(className))
            {
                studentsQuery = studentsQuery.Where(s => s.ClassName == className);
            }

            var students = await studentsQuery.ToListAsync();
            ViewBag.CanDeleteStudents = UserHasDeleteStudentPermission();
            ViewBag.Query = q;
            ViewBag.SelectedClass = className;
            ViewBag.Classes = _context.Students.Select(s => s.ClassName).Distinct().OrderBy(c => c).ToList();
            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? q, string? className)
        {
            var studentsQuery = _context.Students.Include(s => s.CreatedByUser).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var lower = q.ToLower();
                studentsQuery = studentsQuery.Where(s => ((s.FirstName ?? string.Empty).ToLower().Contains(lower))
                                                    || ((s.LastName ?? string.Empty).ToLower().Contains(lower))
                                                    || ((s.Email ?? string.Empty).ToLower().Contains(lower)));
            }
            if (!string.IsNullOrWhiteSpace(className))
            {
                studentsQuery = studentsQuery.Where(s => s.ClassName == className);
            }

            var students = await studentsQuery.ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,FirstName,MiddleInitial,LastName,DateOfBirth,Gender,CaseNumber,ClassName,Ethnicity,Email,Eligibility,StudentDisability,AcademicNeed,FirstEnrollmentDate,DateOfFirstProjectService,CollegeGradeLevelEntryIntoProject,EnrollmentStatusInAcademicYearFirstServed,StudentCohortYear,ParticipantStatusDuringAcademicYearBeingReported,EnrollmentStatusAtEndOfAcademicYear,AcademicStanding,CumulativeGPA,CollegeGradeLevelAtBeginningOfAcademicYear,CollegeGradeLevelAtEndOfAcademicYear,DateOfLastProjectService,ReasonForWithdrawalOrNotReturning,Transfers,UndergraduateDegreeCertificateCompletedAtGranteeInstitution,DateOfUndergraduateDegreeCertificate,FieldOfStudyDegreeCertificateEarned,PersistenceStatusAtBeginningOfAcademicYear,AmountOfFinancialAidReceived,AmountOfSSSGrantAidAwarded,ParticipantNameChange");
            foreach (var s in students)
            {
                // Escape quotes and commas by wrapping fields in double quotes and escaping existing quotes
                string Escape(string? value)
                {
                    if (value == null) return string.Empty;
                    return '"' + value.Replace("\"", "\"\"") + '"';
                }

                sb.AppendLine($"{s.Id},{Escape(s.FirstName)},{Escape(s.MiddleInitial)},{Escape(s.LastName)},{Escape(s.DateOfBirth?.ToString("MM/dd/yyyy"))},{Escape(s.Gender)},{Escape(s.CaseNumber)},{Escape(s.ClassName)},{Escape(s.Ethnicity)},{Escape(s.Email)},{Escape(s.Eligibility)},{Escape(s.StudentDisability)},{Escape(s.AcademicNeed)},{Escape(s.FirstEnrollmentDate?.ToString("MM/dd/yyyy"))},{Escape(s.DateOfFirstProjectService?.ToString("MM/dd/yyyy"))},{Escape(s.CollegeGradeLevelEntryIntoProject)},{Escape(s.EnrollmentStatusInAcademicYearFirstServed)},{Escape(s.StudentCohortYear)},{Escape(s.ParticipantStatusDuringAcademicYearBeingReported)},{Escape(s.EnrollmentStatusAtEndOfAcademicYear)},{Escape(s.AcademicStanding)},{Escape(s.CumulativeGPA?.ToString())},{Escape(s.CollegeGradeLevelAtBeginningOfAcademicYear)},{Escape(s.CollegeGradeLevelAtEndOfAcademicYear)},{Escape(s.DateOfLastProjectService?.ToString("MM/dd/yyyy"))},{Escape(s.ReasonForWithdrawalOrNotReturning)},{Escape(s.Transfers)},{Escape(s.UndergraduateDegreeCertificateCompletedAtGranteeInstitution)},{Escape(s.DateOfUndergraduateDegreeCertificate?.ToString("MM/dd/yyyy"))},{Escape(s.FieldOfStudyDegreeCertificateEarned)},{Escape(s.PersistenceStatusAtBeginningOfAcademicYear)},{Escape(s.AmountOfFinancialAidReceived?.ToString())},{Escape(s.AmountOfSSSGrantAidAwarded?.ToString())},{Escape(s.ParticipantNameChange)}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "students.csv");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.CreatedByUser)
                .Include(s => s.StudentClasses)
                .FirstOrDefaultAsync(s => s.Id == id.Value);
            if (student == null) return NotFound();

            return View(student);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Student student, IFormFile? profilePicture)
        {
            if (!ModelState.IsValid) return View(student);

            // Handle profile picture upload
            if (profilePicture != null && profilePicture.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await profilePicture.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();
                
                // Resize the image
                student.ProfilePicture = ImageHelper.ResizeImage(imageData);
            }

            // record who created the student (if available)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            student.CreatedByUserId = userId;

            // Auto-generate unique case number
            student.CaseNumber = await GenerateUniqueCaseNumberAsync();
            
            // Get currently selected batch year from TempData (set in Directors page)
            var currentBatchYear = TempData.Peek("CurrentBatchYear") as string;
            TempData.Keep("CurrentBatchYear"); // Keep TempData for subsequent requests
            
            // Get DirectorsPageInfo for the current batch year
            DirectorsPageInfo? directorsInfo = null;
            if (!string.IsNullOrEmpty(currentBatchYear) && currentBatchYear != "Creating New...")
            {
                directorsInfo = await _context.DirectorsPageInfos
                    .FirstOrDefaultAsync(d => d.BatchYear == currentBatchYear);
            }
            
            // If no batch year selected, fall back to most recent
            if (directorsInfo == null)
            {
                directorsInfo = await _context.DirectorsPageInfos
                    .Where(d => !string.IsNullOrEmpty(d.BatchYear))
                    .OrderByDescending(d => d.BatchYear)
                    .FirstOrDefaultAsync();
            }
            
            // Set batch year and PR/Award number
            if (directorsInfo != null)
            {
                student.BatchYear = directorsInfo.BatchYear;
                student.PRAwardNumber = directorsInfo.PRAwardNumber;
            }

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.Include(s => s.CreatedByUser).FirstOrDefaultAsync(s => s.Id == id.Value);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Student student, IFormFile? profilePicture)
        {
            if (id != student.Id) return BadRequest();
            if (!ModelState.IsValid) return View(student);

            try
            {
                // preserve CreatedByUserId and CaseNumber if not provided
                var existing = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                if (existing != null)
                {
                    student.CreatedByUserId = existing.CreatedByUserId;
                    student.CaseNumber = existing.CaseNumber; // Preserve case number
                    student.BatchYear = existing.BatchYear; // Preserve batch year
                    student.PRAwardNumber = existing.PRAwardNumber; // Preserve PR/Award number
                    
                    // Handle profile picture upload
                    if (profilePicture != null && profilePicture.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await profilePicture.CopyToAsync(memoryStream);
                        var imageData = memoryStream.ToArray();
                        
                        // Resize the image
                        student.ProfilePicture = ImageHelper.ResizeImage(imageData);
                    }
                    else
                    {
                        // Preserve existing profile picture if no new one uploaded
                        student.ProfilePicture = existing.ProfilePicture;
                    }
                }

                _context.Students.Update(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = student.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Students.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            if (!UserHasDeleteStudentPermission())
            {
                return Forbid();
            }

            var student = await _context.Students.Include(s => s.CreatedByUser).FirstOrDefaultAsync(s => s.Id == id.Value);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!UserHasDeleteStudentPermission())
            {
                return Forbid();
            }

            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        
        [HttpGet]
        public async Task<IActionResult> GetProfilePicture(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student?.ProfilePicture == null || student.ProfilePicture.Length == 0)
            {
                return NotFound();
            }
            
            return File(student.ProfilePicture, "image/jpeg");
        }

        [HttpPost]
        public async Task<IActionResult> AddClass([FromBody] StudentClass studentClass)
        {
            try
            {
                if (string.IsNullOrEmpty(studentClass.ClassName))
                {
                    return Json(new { success = false, message = "Class name is required" });
                }

                _context.StudentClasses.Add(studentClass);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditClass([FromBody] StudentClass studentClass)
        {
            try
            {
                if (string.IsNullOrEmpty(studentClass.ClassName))
                {
                    return Json(new { success = false, message = "Class name is required" });
                }

                var existingClass = await _context.StudentClasses.FindAsync(studentClass.Id);
                if (existingClass == null)
                {
                    return Json(new { success = false, message = "Class not found" });
                }

                existingClass.ClassName = studentClass.ClassName;
                existingClass.Instructor = studentClass.Instructor;
                existingClass.Schedule = studentClass.Schedule;
                existingClass.Semester = studentClass.Semester;
                existingClass.Grade = studentClass.Grade;

                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClass(int id)
        {
            try
            {
                var studentClass = await _context.StudentClasses.FindAsync(id);
                if (studentClass == null)
                {
                    return Json(new { success = false, message = "Class not found" });
                }

                _context.StudentClasses.Remove(studentClass);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Tutoring(int classId)
        {
            var studentClass = await _context.StudentClasses
                .Include(sc => sc.Student)
                .Include(sc => sc.TutoringNotes)
                    .ThenInclude(tn => tn.CreatedByUser)
                .FirstOrDefaultAsync(sc => sc.Id == classId);

            if (studentClass == null)
            {
                return NotFound();
            }

            return View(studentClass);
        }

        [HttpPost]
        public async Task<IActionResult> AddTutoringNote([FromBody] TutoringNoteDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return Json(new { success = false, message = "Invalid data" });
                }

                if (string.IsNullOrEmpty(dto.Notes))
                {
                    return Json(new { success = false, message = "Notes are required" });
                }

                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var tutoringNote = new TutoringNote
                {
                    StudentClassId = dto.StudentClassId,
                    Notes = dto.Notes,
                    NoteDate = dto.NoteDate,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.Now
                };

                _context.TutoringNotes.Add(tutoringNote);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class TutoringNoteDto
        {
            public int StudentClassId { get; set; }
            public string Notes { get; set; } = string.Empty;
            public DateTime NoteDate { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTutoringNote(int id)
        {
            try
            {
                var note = await _context.TutoringNotes.FindAsync(id);
                if (note == null)
                {
                    return Json(new { success = false, message = "Note not found" });
                }

                _context.TutoringNotes.Remove(note);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
