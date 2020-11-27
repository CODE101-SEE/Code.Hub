using System;
using Code.Hub.Core.Dependency;
using Code.Hub.Core.WorkProviders.Providers;
using Code.Hub.Shared.WorkProviders;
using Microsoft.Extensions.DependencyInjection;

namespace Code.Hub.Core.WorkProviders
{
    public static class WorkProviderRegistrationExtensions
    {
        public static void AddWorkProviders(this IServiceCollection services, Action<WorkProviderOptions> configureOptions = null)
        {
            services.AddTransient<IWorkProviderFactory, WorkProviderFactory>();
            services.RegisterServicesFromAssembly<IWorkProvider>(DependencyType.Scoped);

            services.AddOptions<WorkProviderOptions>().Configure(options =>
            {
                configureOptions?.Invoke(options);
                
                options.AddMapping<ZammadWorkProvider>(WorkProviderType.Zammad);
            });
        }
    }
}
