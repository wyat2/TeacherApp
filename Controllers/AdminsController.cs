using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Models;
using TeacherApp.Data;
using Microsoft.EntityFrameworkCore;

namespace TeacherApp.Controllers
{
    [Authorize(Roles = "Admin,Director")]
    public class AdminsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        private bool UserHasDeleteAdminsPermission()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return false;
            var roles = _context.RolePermissions.Where(p => p.CanDeleteAdmins).Select(p => p.RoleName).ToList();
            foreach (var r in roles)
            {
                if (User.IsInRole(r)) return true;
            }
            return false;
        }

        private bool UserHasEditAdminsPermission()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return false;
            var roles = _context.RolePermissions.Where(p => p.CanEditAdmins).Select(p => p.RoleName).ToList();
            foreach (var r in roles)
            {
                if (User.IsInRole(r)) return true;
            }
            return false;
        }

        public async Task<IActionResult> Index()
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            ViewBag.CanDelete = UserHasDeleteAdminsPermission();
            ViewBag.CanEdit = UserHasEditAdminsPermission();
            return View(admins);
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
            if (!UserHasDeleteAdminsPermission()) return Forbid();
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            if (!UserHasDeleteAdminsPermission()) return Forbid();
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Null out FK references before deleting to avoid constraint violation
                var students = _context.Students.Where(s => s.CreatedByUserId == id);
                foreach (var student in students)
                    student.CreatedByUserId = null;
                await _context.SaveChangesAsync();

                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string email, string password, string? firstName, string? lastName, string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                if (string.IsNullOrWhiteSpace(email))
                    ModelState.AddModelError("email", "Email is required.");
                if (string.IsNullOrWhiteSpace(password))
                    ModelState.AddModelError("password", "Password is required.");
                    
                ViewBag.Email = email;
                ViewBag.FirstName = firstName;
                ViewBag.LastName = lastName;
                ViewBag.PhoneNumber = phoneNumber;
                return View();
            }

            var user = new ApplicationUser { UserName = email, Email = email, FirstName = firstName, LastName = lastName, PhoneNumber = phoneNumber };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                return RedirectToAction(nameof(Index));
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
            ViewBag.LastName = lastName;
            ViewBag.PhoneNumber = phoneNumber;
            return View();
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            if (!UserHasEditAdminsPermission()) return Forbid();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string? firstName, string? lastName, string? phoneNumber)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            if (!UserHasEditAdminsPermission()) return Forbid();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.FirstName = firstName;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);

            return View(user);
        }

        // Tools Section
        public async Task<IActionResult> Tools()
        {
            var dropdownFields = new List<DropdownFieldViewModel>
            {
                new DropdownFieldViewModel
                {
                    FieldName = "ContactType",
                    DisplayName = "Contact Type",
                    Values = await _context.DropdownValues
                        .Where(d => d.FieldName == "ContactType")
                        .OrderBy(d => d.DisplayOrder)
                        .ToListAsync()
                }
            };

            return View(dropdownFields);
        }

        [HttpPost]
        public async Task<IActionResult> AddDropdownValue([FromBody] DropdownValueDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Value))
                {
                    return Json(new { success = false, message = "Value is required" });
                }

                var maxOrder = await _context.DropdownValues
                    .Where(d => d.FieldName == dto.FieldName)
                    .MaxAsync(d => (int?)d.DisplayOrder) ?? 0;

                var dropdownValue = new DropdownValue
                {
                    FieldName = dto.FieldName,
                    Value = dto.Value,
                    DisplayOrder = maxOrder + 1
                };

                _context.DropdownValues.Add(dropdownValue);
                await _context.SaveChangesAsync();

                return Json(new { success = true, id = dropdownValue.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDropdownValue([FromBody] DropdownValueDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Value))
                {
                    return Json(new { success = false, message = "Value is required" });
                }

                var dropdownValue = await _context.DropdownValues.FindAsync(dto.Id);
                if (dropdownValue == null)
                {
                    return Json(new { success = false, message = "Value not found" });
                }

                dropdownValue.Value = dto.Value;
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDropdownValue(int id, string fieldName)
        {
            try
            {
                var dropdownValue = await _context.DropdownValues.FindAsync(id);
                if (dropdownValue == null)
                {
                    return Json(new { success = false, message = "Value not found" });
                }

                var valueToDelete = dropdownValue.Value;

                // If this is ContactType, update any contacts using this value to "-"
                if (fieldName == "ContactType")
                {
                    var contactsToUpdate = await _context.Contacts
                        .Where(c => c.ContactType == valueToDelete)
                        .ToListAsync();

                    foreach (var contact in contactsToUpdate)
                    {
                        contact.ContactType = "-";
                    }
                }

                _context.DropdownValues.Remove(dropdownValue);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReorderDropdownValues([FromBody] List<int> orderedIds)
        {
            try
            {
                for (int i = 0; i < orderedIds.Count; i++)
                {
                    var dropdownValue = await _context.DropdownValues.FindAsync(orderedIds[i]);
                    if (dropdownValue != null)
                    {
                        dropdownValue.DisplayOrder = i;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class DropdownFieldViewModel
        {
            public string FieldName { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public List<DropdownValue> Values { get; set; } = new();
        }

        public class DropdownValueDto
        {
            public int Id { get; set; }
            public string FieldName { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }
    }
}
