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
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            IAuditLogService auditLog,
            ApplicationDbContext context,
            IWebHostEnvironment env,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditLog = auditLog;
            _context = context;
            _env = env;
            _configuration = configuration;
            _emailService = emailService;
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Always return success to prevent email enumeration
            // Find user by email
            var user = await _userManager.FindByEmailAsync(model.Email);
            
            if (user != null)
            {
                // Generate temporary password
                var tempPassword = GenerateTemporaryPassword();
                
                // Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, tempPassword);
                
                if (result.Succeeded)
                {
                    // Log the password reset attempt
                    await _auditLog.LogAsync(
                        action: "Password Reset",
                        entityType: "User",
                        entityId: null,
                        details: $"Temporary password generated for {user.Email}"
                    );

                    // Send email with temporary password
                    try
                    {
                        var emailBody = $@"
                            <html>
                            <body style='font-family: Arial, sans-serif;'>
                                <h2>Password Reset Request</h2>
                                <p>Hello,</p>
                                <p>You have requested to reset your password. Your temporary password is:</p>
                                <h3 style='background-color: #f0f0f0; padding: 10px; display: inline-block;'>{tempPassword}</h3>
                                <p>Please log in with this temporary password and change it immediately.</p>
                                <p><strong>Important:</strong> This password is temporary and should be changed as soon as possible for security reasons.</p>
                                <p>If you did not request this password reset, please contact support immediately.</p>
                                <br/>
                                <p>Best regards,<br/>TeacherApp Support Team</p>
                            </body>
                            </html>
                        ";

                        await _emailService.SendEmailAsync(
                            model.Email,
                            "Password Reset - Temporary Password",
                            emailBody
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't reveal it to the user
                        Console.WriteLine($"Failed to send email: {ex.Message}");
                    }
                }
            }

            // Always redirect to confirmation page (security best practice)
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var random = new Random();
            var password = new char[12];
            
            // Ensure at least one uppercase, lowercase, digit, and special char
            password[0] = chars[random.Next(0, 23)]; // Uppercase
            password[1] = chars[random.Next(23, 46)]; // Lowercase
            password[2] = chars[random.Next(46, 55)]; // Digit
            password[3] = chars[random.Next(55, chars.Length)]; // Special
            
            // Fill the rest randomly
            for (int i = 4; i < 12; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            
            // Shuffle the password
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            // Demo mode: reset database on logout
            try
            {
                var connectionString = _configuration.GetConnectionString("PostgreSqlConnection");
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "DROP SCHEMA public CASCADE; CREATE SCHEMA public;";
                await command.ExecuteNonQueryAsync();

                // Recreate all tables immediately so subsequent requests don't hit missing schema objects.
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting database: {ex.Message}");
            }

            // Remove setup flag so setup wizard runs again
            var setupFlagPath = Path.Combine(_env.ContentRootPath, ".setup-complete");
            if (System.IO.File.Exists(setupFlagPath))
            {
                System.IO.File.Delete(setupFlagPath);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
