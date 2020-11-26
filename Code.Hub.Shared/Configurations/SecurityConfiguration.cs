using Microsoft.AspNetCore.Identity;

namespace Code.Hub.Shared.Configurations
{
    public class SecurityConfiguration : PasswordOptions
    {
        // Lockout settings.
        public int LockoutDefaultTimeSpanMinutes { get; set; }
        public int LockoutMaxFailedAccessAttempts { get; set; }
        public bool LockoutAllowedForNewUsers { get; set; }

    }
}
