using Microsoft.AspNetCore.Identity;
using TeacherApp.Models;

namespace TeacherApp.Services
{
    public interface ISetupWizardService
    {
        Task<bool> IsFirstRunAsync();
        Task CompleteSetupAsync(string adminEmail, string adminPassword, bool createSampleData);
    }

    public class SetupWizardService : ISetupWizardService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;
        private readonly string _setupFlagPath;

        public SetupWizardService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
            _setupFlagPath = Path.Combine(_env.ContentRootPath, ".setup-complete");
        }

        public async Task<bool> IsFirstRunAsync()
        {
            // Check if setup flag file exists
            if (File.Exists(_setupFlagPath))
                return false;

            // Also check if any admin users exist
            var adminRole = await _roleManager.FindByNameAsync("Admin");
            if (adminRole != null)
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count > 0)
                {
                    // Mark setup as complete even if flag doesn't exist
                    await CreateSetupFlagAsync();
                    return false;
                }
            }

            return true;
        }

        public async Task CompleteSetupAsync(string adminEmail, string adminPassword, bool createSampleData)
        {
            // Create Admin role if it doesn't exist
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Create or update admin user
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Assign Admin role
            if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Mark setup as complete
            await CreateSetupFlagAsync();
        }

        private async Task CreateSetupFlagAsync()
        {
            await File.WriteAllTextAsync(_setupFlagPath, DateTime.UtcNow.ToString("o"));
        }
    }
}
