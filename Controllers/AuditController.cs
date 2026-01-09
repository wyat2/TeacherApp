using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeacherApp.Data;

namespace TeacherApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? userId, string? action, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(a => a.UserId == userId || a.UserEmail.Contains(userId));
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(a => a.Action == action);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= endDate.Value.AddDays(1));
            }

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Take(500)
                .ToListAsync();

            ViewBag.UserId = userId;
            ViewBag.Action = action;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(logs);
        }
    }
}
