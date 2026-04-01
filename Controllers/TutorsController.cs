using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Models;
using TeacherApp.Data;

namespace TeacherApp.Controllers
{
    [Authorize]
    public class TutorsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TutorsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        private bool UserHasDeleteTutorsPermission()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return false;
            var roles = _context.RolePermissions.Where(p => p.CanDeleteTutors).Select(p => p.RoleName).ToList();
            foreach (var r in roles)
            {
                if (User.IsInRole(r)) return true;
            }
            return false;
        }

        private bool UserHasEditTutorsPermission()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return false;
            var roles = _context.RolePermissions.Where(p => p.CanEditTutors).Select(p => p.RoleName).ToList();
            foreach (var r in roles)
            {
                if (User.IsInRole(r)) return true;
            }
            return false;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var list = (await _userManager.GetUsersInRoleAsync("Tutor")).AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                list = list.Where(u => (u.FirstName ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase)
                                     || (u.LastName ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase)
                                     || (u.Email ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase));
            }
            var result = list.ToList();
            ViewBag.CanDelete = UserHasDeleteTutorsPermission();
            ViewBag.CanEdit = UserHasEditTutorsPermission();
            ViewBag.Query = q;
            return View(result);
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
            if (!UserHasDeleteTutorsPermission()) return Forbid();
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
            if (!UserHasDeleteTutorsPermission()) return Forbid();
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
                await _userManager.AddToRoleAsync(user, "Tutor");
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
            if (!UserHasEditTutorsPermission()) return Forbid();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string? firstName, string? lastName, string? phoneNumber)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            if (!UserHasEditTutorsPermission()) return Forbid();
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
    }
}
