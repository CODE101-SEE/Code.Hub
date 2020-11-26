using Code.Hub.Shared.Configurations.DevOps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Code.Hub.App.Startups.Extensions
{
    public static class RegisterConfigurationExtensions
    {
        public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Password Complexity 
            var passwordConfig = RegisterSecurityExtensions.GetSecurityConfiguration(configuration);
            configuration.Bind("SecurityConfiguration", passwordConfig);
            services.AddSingleton(passwordConfig);

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.Configure<DevOpsConfiguration>(configuration.GetSection("DevOpsConfiguration"));

            services.AddSingleton<DevOpsConfiguration>();

            return services;
        }
    }
}
