using Microsoft.AspNetCore.Identity;

namespace expenseTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}