using Microsoft.AspNetCore.Mvc;
using TeacherApp.Services;
using TeacherApp.ViewModels;

namespace TeacherApp.Controllers
{
    public class SetupController : Controller
    {
        private readonly ISetupWizardService _setupWizard;
        private readonly ILogger<SetupController> _logger;

        public SetupController(ISetupWizardService setupWizard, ILogger<SetupController> logger)
        {
            _setupWizard = setupWizard;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Check if setup is already complete
            if (!await _setupWizard.IsFirstRunAsync())
            {
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
