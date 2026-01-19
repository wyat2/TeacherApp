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
        private readonly IAuditLogService _auditLog;
        
        public StudentsController(ApplicationDbContext context, IAuditLogService auditLog)
        {
            _context = context;
            _auditLog = auditLog;
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

        public async Task<IActionResult> Index(string? q, string? standing, string? gpaRange, bool showAll = false)
        {
            var studentsQuery = _context.Students.Include(s => s.CreatedByUser).AsQueryable();
            
            // Filter by current batch year unless showAll is true
            if (!showAll)
            {
                var currentBatchYearInfo = _context.DirectorsPageInfos
                    .FirstOrDefault(d => d.IsCurrent && !string.IsNullOrEmpty(d.BatchYear));
                var currentBatchYear = currentBatchYearInfo?.BatchYear;
                
                if (!string.IsNullOrEmpty(currentBatchYear))
                {
                    studentsQuery = studentsQuery.Where(s => s.BatchYear == currentBatchYear);
                }
            }
            
            // Check if this is a search or filter request
            bool isSearch = !string.IsNullOrWhiteSpace(q);
            bool isStandingFilter = !string.IsNullOrWhiteSpace(standing);
            bool isGpaRangeFilter = !string.IsNullOrWhiteSpace(gpaRange);
            
            if (isSearch)
            {
                var lower = q!.ToLower();
                studentsQuery = studentsQuery.Where(s => ((s.FirstName ?? string.Empty).ToLower().Contains(lower))
                                                    || ((s.LastName ?? string.Empty).ToLower().Contains(lower))
                                                    || ((s.Email ?? string.Empty).ToLower().Contains(lower)));
            }
            
            if (isStandingFilter)
            {
                studentsQuery = studentsQuery.Where(s => s.AcademicStanding == standing);
            }
            
            // Filter by GPA range if specified
            if (isGpaRangeFilter)
            {
                var rangeParts = gpaRange!.Split('-');
                if (rangeParts.Length == 2 && 
                    decimal.TryParse(rangeParts[0], out var minGpa) && 
                    decimal.TryParse(rangeParts[1], out var maxGpa))
                {
                    studentsQuery = studentsQuery.Where(s => s.CumulativeGPA >= minGpa && s.CumulativeGPA <= maxGpa);
                }
            }

            var students = await studentsQuery.ToListAsync();
            
            // If not searching or filtering, calculate histogram data for each academic standing
            if (!isSearch && !isStandingFilter && !isGpaRangeFilter && !showAll)
            {
                var academicStandings = new[] { "Freshman", "Sophomore", "Junior", "Senior", "Graduate Student" };
                var histogramData = new Dictionary<string, Dictionary<string, int>>();
                
                foreach (var standingType in academicStandings)
                {
                    var standingStudents = students.Where(s => s.AcademicStanding == standingType).ToList();
                    var gpaDistribution = new Dictionary<string, int>
                    {
                        { "0.0-1.0", standingStudents.Count(s => s.CumulativeGPA >= 0 && s.CumulativeGPA < 1.0m) },
                        { "1.0-2.0", standingStudents.Count(s => s.CumulativeGPA >= 1.0m && s.CumulativeGPA < 2.0m) },
                        { "2.0-3.0", standingStudents.Count(s => s.CumulativeGPA >= 2.0m && s.CumulativeGPA < 3.0m) },
                        { "3.0-4.0", standingStudents.Count(s => s.CumulativeGPA >= 3.0m && s.CumulativeGPA <= 4.0m) }
                    };
                    histogramData[standingType] = gpaDistribution;
                }
                
                ViewBag.HistogramData = histogramData;
                ViewBag.TotalStudentsByStanding = academicStandings.ToDictionary(
                    standingType => standingType,
                    standingType => students.Count(s => s.AcademicStanding == standingType)
                );
            }
            
            ViewBag.CanDeleteStudents = UserHasDeleteStudentPermission();
            ViewBag.Query = q;
            ViewBag.IsSearch = isSearch;
            ViewBag.Standing = standing;
            ViewBag.IsStandingFilter = isStandingFilter;
            ViewBag.GpaRange = gpaRange;
            ViewBag.IsGpaRangeFilter = isGpaRangeFilter;
            ViewBag.ShowAll = showAll;
            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? q)
        {
            var studentsQuery = _context.Students.Include(s => s.CreatedByUser).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var lower = q.ToLower();
                studentsQuery = studentsQuery.Where(s => ((s.FirstName ?? string.Empty).ToLower().Contains(lower))
                                                    || ((s.LastName ?? string.Empty).ToLower().Contains(lower))
                                                    || ((s.Email ?? string.Empty).ToLower().Contains(lower)));
            }

            var students = await studentsQuery.ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Id,FirstName,MiddleInitial,LastName,DateOfBirth,Gender,CaseNumber,Ethnicity,Email,Eligibility,StudentDisability,AcademicNeed,FirstEnrollmentDate,DateOfFirstProjectService,CollegeGradeLevelEntryIntoProject,EnrollmentStatusInAcademicYearFirstServed,StudentCohortYear,ParticipantStatusDuringAcademicYearBeingReported,EnrollmentStatusAtEndOfAcademicYear,AcademicStanding,CumulativeGPA,CollegeGradeLevelAtBeginningOfAcademicYear,CollegeGradeLevelAtEndOfAcademicYear,DateOfLastProjectService,ReasonForWithdrawalOrNotReturning,Transfers,UndergraduateDegreeCertificateCompletedAtGranteeInstitution,DateOfUndergraduateDegreeCertificate,FieldOfStudyDegreeCertificateEarned,PersistenceStatusAtBeginningOfAcademicYear,AmountOfFinancialAidReceived,AmountOfSSSGrantAidAwarded,ParticipantNameChange");
            foreach (var s in students)
            {
                // Escape quotes and commas by wrapping fields in double quotes and escaping existing quotes
                string Escape(string? value)
                {
                    if (value == null) return string.Empty;
                    return '"' + value.Replace("\"", "\"\"") + '"';
                }
                
                string FormatDate(DateTime? date)
                {
                    return date?.ToString("MM/dd/yyyy") ?? "00/00/0000";
                }

                sb.AppendLine($"{s.Id},{Escape(s.FirstName)},{Escape(s.MiddleInitial)},{Escape(s.LastName)},{Escape(FormatDate(s.DateOfBirth))},{Escape(s.Gender)},{Escape(s.CaseNumber)},{Escape(s.Ethnicity)},{Escape(s.Email)},{Escape(s.Eligibility)},{Escape(s.StudentDisability)},{Escape(s.AcademicNeed)},{Escape(FormatDate(s.FirstEnrollmentDate))},{Escape(FormatDate(s.DateOfFirstProjectService))},{Escape(s.CollegeGradeLevelEntryIntoProject)},{Escape(s.EnrollmentStatusInAcademicYearFirstServed)},{Escape(s.StudentCohortYear)},{Escape(s.ParticipantStatusDuringAcademicYearBeingReported)},{Escape(s.EnrollmentStatusAtEndOfAcademicYear)},{Escape(s.AcademicStanding)},{Escape(s.CumulativeGPA?.ToString())},{Escape(s.CollegeGradeLevelAtBeginningOfAcademicYear)},{Escape(s.CollegeGradeLevelAtEndOfAcademicYear)},{Escape(FormatDate(s.DateOfLastProjectService))},{Escape(s.ReasonForWithdrawalOrNotReturning)},{Escape(s.Transfers)},{Escape(s.UndergraduateDegreeCertificateCompletedAtGranteeInstitution)},{Escape(FormatDate(s.DateOfUndergraduateDegreeCertificate))},{Escape(s.FieldOfStudyDegreeCertificateEarned)},{Escape(s.PersistenceStatusAtBeginningOfAcademicYear)},{Escape(s.AmountOfFinancialAidReceived?.ToString())},{Escape(s.AmountOfSSSGrantAidAwarded?.ToString())},{Escape(s.ParticipantNameChange)}");
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
                .Include(s => s.Contacts)
                    .ThenInclude(c => c.CreatedByUser)
                .FirstOrDefaultAsync(s => s.Id == id.Value);
            if (student == null) return NotFound();

            // Load dropdown values for Contact Type
            ViewBag.ContactTypes = await _context.DropdownValues
                .Where(d => d.FieldName == "ContactType")
                .OrderBy(d => d.DisplayOrder)
                .Select(d => d.Value)
                .ToListAsync();

            // Audit log - view student record
            await _auditLog.LogAsync("View", "Student", student.Id, 
                $"Viewed student record: {student.FirstName} {student.LastName} (Case: {student.CaseNumber})");

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

            // Ensure names are uppercase and trimmed
            student.FirstName = student.FirstName?.ToUpper().Trim() ?? string.Empty;
            student.LastName = student.LastName?.ToUpper().Trim() ?? string.Empty;

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
            
            // Audit log - create student record
            await _auditLog.LogAsync("Create", "Student", student.Id, 
                $"Created student record: {student.FirstName} {student.LastName} (Case: {student.CaseNumber})");
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.Include(s => s.CreatedByUser).FirstOrDefaultAsync(s => s.Id == id.Value);
            if (student == null) return NotFound();

            // Audit log - access edit form
            await _auditLog.LogAsync("AccessEdit", "Student", student.Id, 
                $"Accessed edit form for student: {student.FirstName} {student.LastName} (Case: {student.CaseNumber})");

            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Student student, IFormFile? profilePicture)
        {
            if (id != student.Id) return BadRequest();
            if (!ModelState.IsValid) return View(student);

            try
            {
                // Ensure names are uppercase and trimmed
                student.FirstName = student.FirstName?.ToUpper().Trim() ?? string.Empty;
                student.LastName = student.LastName?.ToUpper().Trim() ?? string.Empty;

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
                
                // Audit log - update student record
                await _auditLog.LogAsync("Update", "Student", student.Id, 
                    $"Updated student record: {student.FirstName} {student.LastName} (Case: {student.CaseNumber})");
                
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
                // Audit log - delete student record
                await _auditLog.LogAsync("Delete", "Student", student.Id, 
                    $"Deleted student record: {student.FirstName} {student.LastName} (Case: {student.CaseNumber})");
                
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

        [HttpPost]
        public async Task<IActionResult> AddContact([FromBody] ContactDto contactDto)
        {
            try
            {
                if (string.IsNullOrEmpty(contactDto.ContactType))
                {
                    return Json(new { success = false, message = "Contact type is required" });
                }

                var contact = new Contact
                {
                    StudentId = contactDto.StudentId,
                    Date = DateTime.Parse(contactDto.Date),
                    ContactType = contactDto.ContactType,
                    Notes = contactDto.Notes,
                    CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                    CreatedAt = DateTime.Now
                };

                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class ContactDto
        {
            public int Id { get; set; }
            public int StudentId { get; set; }
            public string Date { get; set; } = string.Empty;
            public string ContactType { get; set; } = string.Empty;
            public string? Notes { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> EditContact([FromBody] ContactDto contactDto)
        {
            try
            {
                if (string.IsNullOrEmpty(contactDto.ContactType))
                {
                    return Json(new { success = false, message = "Contact type is required" });
                }

                var existingContact = await _context.Contacts.FindAsync(contactDto.Id);
                if (existingContact == null)
                {
                    return Json(new { success = false, message = "Contact not found" });
                }

                existingContact.Date = DateTime.Parse(contactDto.Date);
                existingContact.ContactType = contactDto.ContactType;
                existingContact.Notes = contactDto.Notes;

                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteContact(int id)
        {
            try
            {
                var contact = await _context.Contacts.FindAsync(id);
                if (contact == null)
                {
                    return Json(new { success = false, message = "Contact not found" });
                }

                _context.Contacts.Remove(contact);
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
                .Include(sc => sc.Homework)
                    .ThenInclude(hw => hw.CreatedByUser)
                .Include(sc => sc.Tests)
                    .ThenInclude(t => t.CreatedByUser)
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

        [HttpPost]
        public async Task<IActionResult> AddHomework([FromBody] HomeworkDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return Json(new { success = false, message = "Invalid data" });
                }

                if (string.IsNullOrEmpty(dto.Name))
                {
                    return Json(new { success = false, message = "Homework name is required" });
                }

                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var homework = new Homework
                {
                    StudentClassId = dto.StudentClassId,
                    Name = dto.Name,
                    Date = dto.Date,
                    Score = dto.Score,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.Now
                };

                _context.Homework.Add(homework);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class HomeworkDto
        {
            public int StudentClassId { get; set; }
            public string Name { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public string? Score { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHomework(int id)
        {
            try
            {
                var homework = await _context.Homework.FindAsync(id);
                if (homework == null)
                {
                    return Json(new { success = false, message = "Homework not found" });
                }

                _context.Homework.Remove(homework);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddTest([FromBody] TestDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return Json(new { success = false, message = "Invalid data" });
                }

                if (string.IsNullOrEmpty(dto.Name))
                {
                    return Json(new { success = false, message = "Test name is required" });
                }

                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }

                var test = new Test
                {
                    StudentClassId = dto.StudentClassId,
                    Name = dto.Name,
                    Date = dto.Date,
                    Score = dto.Score,
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.Now
                };

                _context.Tests.Add(test);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class TestDto
        {
            public int StudentClassId { get; set; }
            public string Name { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public string? Score { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTest(int id)
        {
            try
            {
                var test = await _context.Tests.FindAsync(id);
                if (test == null)
                {
                    return Json(new { success = false, message = "Test not found" });
                }

                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateHomework([FromBody] UpdateHomeworkDto dto)
        {
            try
            {
                if (dto == null || dto.Id == 0)
                {
                    return Json(new { success = false, message = "Invalid data" });
                }

                var homework = await _context.Homework.FindAsync(dto.Id);
                if (homework == null)
                {
                    return Json(new { success = false, message = "Homework not found" });
                }

                homework.Name = dto.Name;
                homework.Date = dto.Date;
                homework.Score = dto.Score;

                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class UpdateHomeworkDto
        {
            public int Id { get; set; }
            public int StudentClassId { get; set; }
            public string Name { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public string? Score { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTest([FromBody] UpdateTestDto dto)
        {
            try
            {
                if (dto == null || dto.Id == 0)
                {
                    return Json(new { success = false, message = "Invalid data" });
                }

                var test = await _context.Tests.FindAsync(dto.Id);
                if (test == null)
                {
                    return Json(new { success = false, message = "Test not found" });
                }

                test.Name = dto.Name;
                test.Date = dto.Date;
                test.Score = dto.Score;

                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class UpdateTestDto
        {
            public int Id { get; set; }
            public int StudentClassId { get; set; }
            public string Name { get; set; } = string.Empty;
            public DateTime Date { get; set; }
            public string? Score { get; set; }
        }
    }
}
