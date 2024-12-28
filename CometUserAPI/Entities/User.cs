using Microsoft.AspNetCore.Identity;

namespace CometUserAPI.Entities
{
    public class User : IdentityUser
    {
        public string? initials { get; set; }
    }
}
