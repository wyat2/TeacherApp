using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace TeacherApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // If user is authenticated, redirect to Students page
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Students");
            }
            
            // Otherwise, redirect to login
            return RedirectToAction("Login", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;
            
            ViewBag.ErrorMessage = exception?.Message ?? "An error occurred while processing your request.";
            ViewBag.StackTrace = exception?.StackTrace;
            
            return View();
        }
    }
}
