using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherApp.Data;
using TeacherApp.Models;

namespace TeacherApp.Controllers
{
    [Authorize(Roles = "Admin,Director")]
    public class PermissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionsController(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = _roleManager.Roles.Select(r => r.Name!).ToList();
            var perms = await _context.RolePermissions.ToListAsync();

            var model = roles.Select(r => new RolePermission
            {
                RoleName = r,
                Id = perms.FirstOrDefault(p => p.RoleName == r)?.Id ?? 0,
                CanDeleteStudents = perms.FirstOrDefault(p => p.RoleName == r)?.CanDeleteStudents ?? false,
                CanManageAdmins = perms.FirstOrDefault(p => p.RoleName == r)?.CanManageAdmins ?? false,
                CanDeleteAdmins = perms.FirstOrDefault(p => p.RoleName == r)?.CanDeleteAdmins ?? false,
                CanDeleteDirectors = perms.FirstOrDefault(p => p.RoleName == r)?.CanDeleteDirectors ?? false,
                CanDeleteTutors = perms.FirstOrDefault(p => p.RoleName == r)?.CanDeleteTutors ?? false
                ,CanEditAdmins = perms.FirstOrDefault(p => p.RoleName == r)?.CanEditAdmins ?? false
                ,CanEditDirectors = perms.FirstOrDefault(p => p.RoleName == r)?.CanEditDirectors ?? false
                ,CanEditTutors = perms.FirstOrDefault(p => p.RoleName == r)?.CanEditTutors ?? false
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Edit(string role)
        {
            if (string.IsNullOrEmpty(role)) return BadRequest();
            var perm = await _context.RolePermissions.FirstOrDefaultAsync(p => p.RoleName == role);
            if (perm == null)
            {
                perm = new RolePermission { RoleName = role };
            }
            return View(perm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RolePermission model)
        {
            if (!ModelState.IsValid) return View(model);

            var existing = await _context.RolePermissions.FirstOrDefaultAsync(p => p.RoleName == model.RoleName);
            if (existing == null)
            {
                _context.RolePermissions.Add(model);
            }
            else
            {
                existing.CanDeleteStudents = model.CanDeleteStudents;
                existing.CanManageAdmins = model.CanManageAdmins;
                existing.CanDeleteAdmins = model.CanDeleteAdmins;
                existing.CanDeleteDirectors = model.CanDeleteDirectors;
                existing.CanDeleteTutors = model.CanDeleteTutors;
                existing.CanEditAdmins = model.CanEditAdmins;
                existing.CanEditDirectors = model.CanEditDirectors;
                existing.CanEditTutors = model.CanEditTutors;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
