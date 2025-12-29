using Microsoft.AspNetCore.Identity;

namespace TeacherApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        // Whether this director is the primary director (only one director should be primary)
        public bool Primary { get; set; }
        // PhoneNumber is already provided by IdentityUser
    }
}
