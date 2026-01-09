using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Models;
using TeacherApp.Data;

namespace TeacherApp.Controllers
{
    [Authorize(Roles = "Admin,Director")]
    public class DirectorsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DirectorsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        private bool UserHasDeleteDirectorsPermission()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return false;
            var roles = _context.RolePermissions.Where(p => p.CanDeleteDirectors).Select(p => p.RoleName).ToList();
            foreach (var r in roles)
            {
                if (User.IsInRole(r)) return true;
            }
            return false;
        }

        private bool UserHasEditDirectorsPermission()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return false;
            var roles = _context.RolePermissions.Where(p => p.CanEditDirectors).Select(p => p.RoleName).ToList();
            foreach (var r in roles)
            {
                if (User.IsInRole(r)) return true;
            }
            return false;
        }

        public async Task<IActionResult> Index(string? q, string? selectedBatchYear, bool newBatchYear = false)
        {
            var directors = (await _userManager.GetUsersInRoleAsync("Director")).AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                directors = directors.Where(u => (u.FirstName ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase)
                                               || (u.LastName ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase)
                                               || (u.Email ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase));
            }
            var list = directors.ToList();
            ViewBag.CanDelete = UserHasDeleteDirectorsPermission();
            ViewBag.CanEdit = UserHasEditDirectorsPermission();
            
            // Get available batch years
            var allBatchYears = _context.DirectorsPageInfos
                .Where(d => !string.IsNullOrEmpty(d.BatchYear))
                .Select(d => d.BatchYear)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();
            ViewBag.AvailableBatchYears = allBatchYears;
            
            // Determine which batch year to show
            DirectorsPageInfo? info = null;
            
            // If creating a new batch year, show empty form
            if (newBatchYear)
            {
                info = new DirectorsPageInfo();
                ViewBag.SelectedBatchYear = null;
                TempData["CurrentBatchYear"] = "Creating New...";
            }
            else if (!string.IsNullOrEmpty(selectedBatchYear))
            {
                // User explicitly selected a batch year
                info = _context.DirectorsPageInfos
                    .FirstOrDefault(d => d.BatchYear == selectedBatchYear);
                ViewBag.SelectedBatchYear = selectedBatchYear;
                TempData["CurrentBatchYear"] = selectedBatchYear;
            }
            else
            {
                // Check if there's a previously selected batch year in TempData
                var previouslySelected = TempData.Peek("CurrentBatchYear") as string;
                
                if (!string.IsNullOrEmpty(previouslySelected) && previouslySelected != "Creating New...")
                {
                    // Use the previously selected batch year
                    info = _context.DirectorsPageInfos
                        .FirstOrDefault(d => d.BatchYear == previouslySelected);
                    ViewBag.SelectedBatchYear = previouslySelected;
                    TempData.Keep("CurrentBatchYear");
                }
                else if (allBatchYears.Any())
                {
                    // Show the most recent batch year by default
                    var latestYear = allBatchYears.First();
                    info = _context.DirectorsPageInfos
                        .FirstOrDefault(d => d.BatchYear == latestYear);
                    ViewBag.SelectedBatchYear = latestYear;
                    TempData["CurrentBatchYear"] = latestYear;
                }
            }
            
            ViewBag.DirectorsPageInfo = info ?? new TeacherApp.Models.DirectorsPageInfo();
            ViewBag.Query = q;
            
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimary(string id, bool primary)
        {
            if (!UserHasEditDirectorsPermission()) return Forbid();
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // If setting this user to primary, clear Primary flag from all other directors
            if (primary)
            {
                var directors = await _userManager.GetUsersInRoleAsync("Director");
                foreach (var d in directors)
                {
                    if (d.Id != id && d.Primary)
                    {
                        d.Primary = false;
                        await _userManager.UpdateAsync(d);
                    }
                }

                user.Primary = true;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                user.Primary = false;
                await _userManager.UpdateAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSchoolInfo(string? schoolName, string? address, string? schoolPhone, string? currentBatchYear)
        {
            DirectorsPageInfo? info = null;
            
            if (!string.IsNullOrEmpty(currentBatchYear))
            {
                info = _context.DirectorsPageInfos
                    .FirstOrDefault(d => d.BatchYear == currentBatchYear);
            }
            
            if (info == null)
            {
                info = new TeacherApp.Models.DirectorsPageInfo();
                _context.DirectorsPageInfos.Add(info);
            }

            info.SchoolName = schoolName;
            info.Address = address;
            info.SchoolPhone = schoolPhone;

            _context.SaveChanges();
            return RedirectToAction(nameof(Index), new { selectedBatchYear = info.BatchYear });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveDirectorInfo(string? directorName, string? directorEmail, string? directorPhone, string? currentBatchYear)
        {
            DirectorsPageInfo? info = null;
            
            if (!string.IsNullOrEmpty(currentBatchYear))
            {
                info = _context.DirectorsPageInfos
                    .FirstOrDefault(d => d.BatchYear == currentBatchYear);
            }
            
            if (info == null)
            {
                info = new TeacherApp.Models.DirectorsPageInfo();
                _context.DirectorsPageInfos.Add(info);
            }

            info.DirectorName = directorName;
            info.DirectorEmail = directorEmail;
            info.DirectorPhone = directorPhone;

            _context.SaveChanges();
            return RedirectToAction(nameof(Index), new { selectedBatchYear = info.BatchYear });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveGrantInfo(string? grantType, string? prAwardNumber, string? typeOfInstitution, string? projectType, string? gpaScale, string? nameOfGranteeInstitution, decimal? grantBudget, string? batchYear, string? currentBatchYear)
        {
            // Validate PR/Award Number format (must be exactly 11 digits if provided)
            if (!string.IsNullOrWhiteSpace(prAwardNumber))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(prAwardNumber, @"^\d{11}$"))
                {
                    ModelState.AddModelError("prAwardNumber", "PR/Award Number must be exactly 11 digits.");
                    return RedirectToAction(nameof(Index), new { selectedBatchYear = currentBatchYear });
                }
            }

            // Validate Batch Year format (must be exactly 4 digits if provided)
            if (!string.IsNullOrWhiteSpace(batchYear))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(batchYear, @"^\d{4}$"))
                {
                    ModelState.AddModelError("batchYear", "Batch Year must be exactly 4 digits.");
                    return RedirectToAction(nameof(Index), new { selectedBatchYear = currentBatchYear });
                }
            }

            DirectorsPageInfo? info = null;
            
            // Find existing record for the current batch year being edited
            if (!string.IsNullOrEmpty(currentBatchYear))
            {
                info = _context.DirectorsPageInfos
                    .FirstOrDefault(d => d.BatchYear == currentBatchYear);
            }
            
            // If batch year changed, check if new batch year already exists
            if (!string.IsNullOrEmpty(batchYear) && batchYear != currentBatchYear)
            {
                var existingYear = _context.DirectorsPageInfos
                    .Any(d => d.BatchYear == batchYear);
                if (existingYear)
                {
                    TempData["ErrorMessage"] = $"Batch Year {batchYear} already exists. Please choose a different year.";
                    return RedirectToAction(nameof(Index), new { selectedBatchYear = currentBatchYear });
                }
            }
            
            if (info == null)
            {
                info = new TeacherApp.Models.DirectorsPageInfo();
                _context.DirectorsPageInfos.Add(info);
            }

            info.GrantType = grantType;
            info.PRAwardNumber = prAwardNumber;
            info.TypeOfInstitution = typeOfInstitution;
            info.ProjectType = projectType;
            info.GPAScale = gpaScale;
            info.NameOfGranteeInstitution = nameOfGranteeInstitution;
            info.GrantBudget = grantBudget;
            info.BatchYear = batchYear;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index), new { selectedBatchYear = batchYear });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateNewBatchYear()
        {
            // Redirect to Index with newBatchYear flag to show empty form
            return RedirectToAction(nameof(Index), new { newBatchYear = true });
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            if (!UserHasDeleteDirectorsPermission()) return Forbid();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            if (!UserHasDeleteDirectorsPermission()) return Forbid();
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string email, string password, string? firstName, string? middleInitial, string? lastName, string? phoneNumber, bool isProjectDirector = false, bool isDataEntryPerson = false)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                if (string.IsNullOrWhiteSpace(email))
                    ModelState.AddModelError("email", "Email is required.");
                if (string.IsNullOrWhiteSpace(password))
                    ModelState.AddModelError("password", "Password is required.");
                    
                ViewBag.Email = email;
                ViewBag.FirstName = firstName;
                ViewBag.MiddleInitial = middleInitial;
                ViewBag.LastName = lastName;
                ViewBag.PhoneNumber = phoneNumber;
                ViewBag.IsProjectDirector = isProjectDirector;
                ViewBag.IsDataEntryPerson = isDataEntryPerson;
                return View();
            }

            var user = new ApplicationUser { UserName = email, Email = email, FirstName = firstName, MiddleInitial = middleInitial, LastName = lastName, PhoneNumber = phoneNumber, IsProjectDirector = isProjectDirector, IsDataEntryPerson = isDataEntryPerson };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Director");
                
                // Clear Project Director flag from other directors if this user is being set as Project Director
                if (isProjectDirector)
                {
                    var directors = await _userManager.GetUsersInRoleAsync("Director");
                    foreach (var d in directors)
                    {
                        if (d.Id != user.Id && d.IsProjectDirector)
                        {
                            d.IsProjectDirector = false;
                            await _userManager.UpdateAsync(d);
                        }
                    }
                }
                
                // Clear Data Entry Person flag from other directors if this user is being set as Data Entry Person
                if (isDataEntryPerson)
                {
                    var directors = await _userManager.GetUsersInRoleAsync("Director");
                    foreach (var d in directors)
                    {
                        if (d.Id != user.Id && d.IsDataEntryPerson)
                        {
                            d.IsDataEntryPerson = false;
                            await _userManager.UpdateAsync(d);
                        }
                    }
                }
                
                return Redirect(Url.Action(nameof(Index)) + "#director");
            }

            foreach (var err in result.Errors)
            {
                if (err.Code.Contains("Password"))
                    ModelState.AddModelError("password", err.Description);
                else if (err.Code.Contains("Email") || err.Code.Contains("UserName"))
                    ModelState.AddModelError("email", err.Description);
                else
                    ModelState.AddModelError(string.Empty, err.Description);
            }

            ViewBag.Email = email;
            ViewBag.FirstName = firstName;
            ViewBag.MiddleInitial = middleInitial;
            ViewBag.LastName = lastName;
            ViewBag.PhoneNumber = phoneNumber;
            ViewBag.IsProjectDirector = isProjectDirector;
            ViewBag.IsDataEntryPerson = isDataEntryPerson;
            return View();
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            if (!UserHasEditDirectorsPermission()) return Forbid();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string? firstName, string? middleInitial, string? lastName, string? phoneNumber, bool isProjectDirector = false, bool isDataEntryPerson = false)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            if (!UserHasEditDirectorsPermission()) return Forbid();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.FirstName = firstName;
            user.MiddleInitial = middleInitial;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;
            user.IsProjectDirector = isProjectDirector;
            user.IsDataEntryPerson = isDataEntryPerson;

            // Clear Project Director flag from other directors if this user is being set as Project Director
            if (isProjectDirector)
            {
                var directors = await _userManager.GetUsersInRoleAsync("Director");
                foreach (var d in directors)
                {
                    if (d.Id != user.Id && d.IsProjectDirector)
                    {
                        d.IsProjectDirector = false;
                        await _userManager.UpdateAsync(d);
                    }
                }
            }
            
            // Clear Data Entry Person flag from other directors if this user is being set as Data Entry Person
            if (isDataEntryPerson)
            {
                var directors = await _userManager.GetUsersInRoleAsync("Director");
                foreach (var d in directors)
                {
                    if (d.Id != user.Id && d.IsDataEntryPerson)
                    {
                        d.IsDataEntryPerson = false;
                        await _userManager.UpdateAsync(d);
                    }
                }
            }

            // Primary handled by dedicated action or checkbox in the Index tab; keep existing behavior here

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Redirect(Url.Action(nameof(Index)) + "#director");
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);

            return View(user);
        }
    }
}
