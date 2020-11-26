using Code.Hub.Shared.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Code.Hub.App.Startups.Extensions
{
    public static class RegisterSecurityExtensions
    {
        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            var security = GetSecurityConfiguration(configuration);

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = security.RequireDigit;
                options.Password.RequireLowercase = security.RequireLowercase;
                options.Password.RequireNonAlphanumeric = security.RequireNonAlphanumeric;
                options.Password.RequireUppercase = security.RequireUppercase;
                options.Password.RequiredLength = security.RequiredLength;
                options.Password.RequiredUniqueChars = security.RequiredUniqueChars;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(security.LockoutDefaultTimeSpanMinutes);
                options.Lockout.MaxFailedAccessAttempts = security.LockoutMaxFailedAccessAttempts;
                options.Lockout.AllowedForNewUsers = security.LockoutAllowedForNewUsers;

                // User settings.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSession();

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(1);

                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            return services;
        }

        public static SecurityConfiguration GetSecurityConfiguration(IConfiguration configuration)
        {
            var security = configuration.GetValue<SecurityConfiguration>("SecurityConfiguration");
            security ??= new SecurityConfiguration
            {
                LockoutAllowedForNewUsers = true,
                LockoutDefaultTimeSpanMinutes = 5,
                LockoutMaxFailedAccessAttempts = 5,
                RequireLowercase = true,
                RequireDigit = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true,
                RequiredLength = 8,
                RequiredUniqueChars = 0
            };

            return security;
        }
    }
}
