using Microsoft.Extensions.DependencyInjection;
using System;

namespace Code.Hub.Core.Dependency
{
    /// <summary>
    /// Add static service resolver to use when dependencies injection is not available
    /// </summary>
    public class ServiceActivator : IServiceActivator
    {
        internal IServiceProvider ServiceProvider;

        /// <summary>
        /// Configure ServiceActivator with full serviceProvider
        /// </summary>
        /// <param name="serviceProvider"></param>
        public void Configure(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public static IServiceActivator Instance { get; }

        static ServiceActivator()
        {
            Instance = new ServiceActivator();
        }

        /// <inheritdoc />
        public IServiceScope GetScope()
        {
            return ServiceProvider?.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }

        /// <inheritdoc />
        public TService GetService<TService>()
        {
            return ServiceProvider?.GetService(typeof(TService)) is TService ? (TService)ServiceProvider?.GetService(typeof(TService)) : default;
        }

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            return ServiceProvider?.GetService(serviceType);
        }

        /// <inheritdoc />
        public TService GetRequiredService<TService>()
        {
            if (ServiceProvider == null)
                throw new InvalidOperationException(nameof(ServiceProvider));
            return ServiceProvider.GetRequiredService<TService>();
        }

        /// <inheritdoc />
        public TService GetRequiredServiceOrNull<TService>()
        {
            return ServiceProvider == null ? default : ServiceProvider.GetRequiredServiceOrNull<TService>();
        }
    }
}
