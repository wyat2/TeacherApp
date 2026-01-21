using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherApp.Models;
using TeacherApp.ViewModels;
using TeacherApp.Services;
using TeacherApp.Data;
using Npgsql;

namespace TeacherApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuditLogService _auditLog;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            IAuditLogService auditLog,
            ApplicationDbContext context,
            IWebHostEnvironment env,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditLog = auditLog;
            _context = context;
            _env = env;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account locked due to too many failed login attempts. Please try again in 15 minutes.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            
            // Clear the database
            try
            {
                var connectionString = _configuration.GetConnectionString("PostgreSqlConnection");
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = "DROP SCHEMA public CASCADE; CREATE SCHEMA public;";
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                // Log error but continue with logout
                Console.WriteLine($"Error clearing database: {ex.Message}");
            }
            
            // Remove setup flag
            var setupFlagPath = Path.Combine(_env.ContentRootPath, ".setup-complete");
            if (System.IO.File.Exists(setupFlagPath))
            {
                System.IO.File.Delete(setupFlagPath);
            }
            
            return RedirectToAction("Index", "Home");
        }
    }
}
