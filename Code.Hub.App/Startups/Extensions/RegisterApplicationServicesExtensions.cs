using Code.Hub.Core.Dependency;
using Code.Hub.Core.Services.Base;
using Code.Hub.Core.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Code.Hub.App.Startups.Extensions
{
    public static class RegisterApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddServicesByConvention(typeof(Startup).Assembly, typeof(CodeHubBaseService).Assembly);
            services.AddScoped<UserService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services;
        }
    }
}
