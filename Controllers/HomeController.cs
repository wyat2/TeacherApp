using Microsoft.AspNetCore.Mvc;

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
    }
}
