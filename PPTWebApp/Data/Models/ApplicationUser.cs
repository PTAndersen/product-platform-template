using Microsoft.AspNetCore.Identity;

namespace PPTWebApp.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required UserProfile Profile { get; set; }
    }
}
