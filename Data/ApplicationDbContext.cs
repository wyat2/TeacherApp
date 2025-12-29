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
        public DbSet<TeacherApp.Models.RolePermission> RolePermissions { get; set; }
        public DbSet<TeacherApp.Models.DirectorsPageInfo> DirectorsPageInfos { get; set; }
    }
}
