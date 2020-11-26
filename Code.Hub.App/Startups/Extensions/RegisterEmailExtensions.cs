using Code.Hub.App.Configurations.Email;
using Code.Hub.Shared.Configurations.Email;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Code.Hub.App.Startups.Extensions
{
    public static class RegisterEmailExtensions
    {
        public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
        {
            //GridSend
            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(configuration.GetSection("SendGrid"));

            //Outlook
            //services.AddTransient<IEmailSender, CustomEmailSender>();
            //services.Configure<CustomEmailSenderSettings>(Configuration.GetSection("CustomEmailSenderSettings"));

            return services;
        }
    }
}
