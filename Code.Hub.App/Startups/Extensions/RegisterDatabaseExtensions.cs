using Code.Hub.App.Areas.Identity;
using Code.Hub.EFCoreData;
using Code.Hub.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Code.Hub.App.Startups.Extensions
{
    public static class RegisterDatabaseExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CodeHubContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<CodeHubUser>()
                .AddRoles<CodeHubRole>()
                .AddEntityFrameworkStores<CodeHubContext>().AddDefaultTokenProviders();

            services.AddScoped<AuthenticationStateProvider, RevalidatingAuthenticationStateProvider<CodeHubUser>>();

            return services;
        }
    }

}
