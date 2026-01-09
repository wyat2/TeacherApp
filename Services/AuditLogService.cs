using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TeacherApp.Data;
using TeacherApp.Models;

namespace TeacherApp.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string entityType, int? entityId, string? details = null);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string entityType, int? entityId, string? details = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var user = httpContext.User;
            if (user?.Identity?.IsAuthenticated != true) return;

            var auditLog = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown",
                UserEmail = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity.Name ?? "Unknown",
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext.Request.Headers["User-Agent"].ToString()
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
