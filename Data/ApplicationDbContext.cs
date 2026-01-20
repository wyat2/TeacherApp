using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeacherApp.Models;

namespace TeacherApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<TutoringNote> TutoringNotes { get; set; }
        public DbSet<Homework> Homework { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<TeacherApp.Models.RolePermission> RolePermissions { get; set; }
        public DbSet<TeacherApp.Models.DirectorsPageInfo> DirectorsPageInfos { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<DropdownValue> DropdownValues { get; set; }

        public override int SaveChanges()
        {
            ConvertDateTimesToUtc();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDateTimesToUtc();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ConvertDateTimesToUtc()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                foreach (var property in entry.Properties)
                {
                    if (property.CurrentValue is DateTime dateTime && dateTime.Kind == DateTimeKind.Unspecified)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    }
                }
            }
        }
    }
}
