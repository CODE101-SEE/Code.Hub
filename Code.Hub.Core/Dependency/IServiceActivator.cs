using Microsoft.Extensions.DependencyInjection;
using System;

namespace Code.Hub.Core.Dependency
{
    public interface IServiceActivator
    {
        /// <summary>
        /// Create a scope where use this ServiceActivator
        /// </summary>
        /// <returns>ServiceScope</returns>
        IServiceScope GetScope();

        /// <summary>
        /// Get service of type <typeparamref name="TService"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service object to get.</typeparam>
        /// <returns>A service object of type <typeparamref name="TService"/> or null if there is no such service.</returns>
        TService GetService<TService>();

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>The service that was produced.</returns>
        object GetService(Type serviceType);

        /// <summary>
        /// Get service of type <typeparamref name="TService"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service object to get.</typeparam>
        /// <returns>A service object of type <typeparamref name="TService"/>.</returns>
        /// <exception cref="InvalidOperationException">There is no service of type <typeparamref name="TService"/>.</exception>
        TService GetRequiredService<TService>();

        /// <summary>
        /// Get service of type <typeparamref name="TService"/> from the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <typeparam name="TService">The type of service object to get.</typeparam>
        /// <returns>A service object of type <typeparamref name="TService"/> or null if there is no such service.</returns>
        TService GetRequiredServiceOrNull<TService>();
    }
}