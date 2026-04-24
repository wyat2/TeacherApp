using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeacherApp.Models;
using TeacherApp.Services;
using TeacherApp.ViewModels;

namespace TeacherApp.Controllers
{
    public class SetupController : Controller
    {
        private readonly ISetupWizardService _setupWizard;
        private readonly ILogger<SetupController> _logger;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private const string DevEmail = "dev@example.com";
        private const string DevPassword = "DevPassword1!";

        public SetupController(
            ISetupWizardService setupWizard,
            ILogger<SetupController> logger,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _setupWizard = setupWizard;
            _logger = logger;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!await _setupWizard.IsFirstRunAsync())
                return RedirectToAction("Index", "Home");

            if (_configuration.GetValue<bool>("DemoMode:Enabled"))
            {
                await _setupWizard.CompleteSetupAsync("Dev", "Director", DevEmail, DevPassword, DateTime.UtcNow.Year.ToString(), createSampleData: true);
                var user = await _userManager.FindByEmailAsync(DevEmail);
                await _signInManager.SignInAsync(user!, isPersistent: false);
                _logger.LogInformation("DemoMode: completed setup and signed in as {Email}", DevEmail);
                return RedirectToAction("Index", "Home");
            }

            return View(new SetupWizardViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(SetupWizardViewModel model)
        {
            // Re-check setup status (security measure)
            if (!await _setupWizard.IsFirstRunAsync())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _setupWizard.CompleteSetupAsync(model.FirstName, model.LastName, model.AdminEmail, model.AdminPassword, model.BatchYear, model.CreateSampleData);
                _logger.LogInformation($"Setup wizard completed. Director user created: {model.FirstName} {model.LastName} ({model.AdminEmail}), Batch Year: {model.BatchYear}");
                return RedirectToAction("Complete");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Setup wizard failed");
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError(string.Empty, $"Setup failed: {errorMessage}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Complete()
        {
            // Verify setup is complete
            if (await _setupWizard.IsFirstRunAsync())
            {
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}
