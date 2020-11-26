using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Code.Hub.Core.Dependency
{
    public static class DependencyRegistrationExtensions
    {
        private static readonly Type[] DependencyInterfaces = { typeof(ITransientDependency), typeof(ISingletonDependency), typeof(IScopedDependency), typeof(IHostedDependency) };

        public static void AddServicesByConvention(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (!assemblies.Any())
            {
                assemblies = new[] { Assembly.GetExecutingAssembly() };
            }

            foreach (Assembly assembly in assemblies)
            {
                services.RegisterServicesFromAssembly<ITransientDependency>(assembly, DependencyType.Transient);
                services.RegisterServicesFromAssembly<ISingletonDependency>(assembly, DependencyType.Singleton);
                services.RegisterServicesFromAssembly<IScopedDependency>(assembly, DependencyType.Scoped);
                services.RegisterServicesFromAssembly<IHostedDependency>(assembly, DependencyType.Hosted);
            }
        }

        public static void RegisterServicesFromAssembly<TInterface>(this IServiceCollection services, DependencyType dependencyType = DependencyType.Transient)
        {
            services.RegisterServicesFromAssembly<TInterface>(typeof(DependencyRegistrationExtensions).Assembly, dependencyType);
        }

        public static void RegisterServicesFromAssembly<TInterface>(this IServiceCollection services, Assembly assembly, DependencyType dependencyType = DependencyType.Transient)
        {
            services.RegisterServicesFromAssembly(typeof(TInterface), assembly, dependencyType);
        }

        private static void RegisterServicesFromAssembly(this IServiceCollection services, Type lookupType, Assembly assembly, DependencyType dependencyType = DependencyType.Transient)
        {
            List<Type> implementingTypes = assembly.GetAllTypesImplementing(lookupType);

            foreach (Type implementationType in implementingTypes)
            {
                Type[] implementationTypeInterfaces = implementationType.GetInterfaces();

                IEnumerable<Type> lookupTypeServices = GetRegistrationTypes(implementationType, lookupType, implementingTypes, implementationTypeInterfaces);

                Type dependencyInterfaceType = implementationTypeInterfaces.FirstOrDefault(s => DependencyInterfaces.Contains(s));

                if (dependencyInterfaceType != null)
                {
                    dependencyType = GetDependencyTypeFromType(dependencyInterfaceType, dependencyType);
                }

                foreach (Type service in lookupTypeServices)
                {
                    ServiceDescriptor descriptor = null;

                    switch (dependencyType)
                    {
                        case DependencyType.Singleton:
                            descriptor = CreateSingletonDescriptor(service, implementationType);
                            break;
                        case DependencyType.Transient:
                            descriptor = CreateTransientDescriptor(service, implementationType);
                            break;
                        case DependencyType.Scoped:
                            descriptor = CreateScopedDescriptor(service, implementationType);
                            break;
                        case DependencyType.Hosted:
                            descriptor = CreateSingletonDescriptor(typeof(IHostedService), implementationType);
                            break;
                    }

                    if (dependencyType == DependencyType.Hosted)
                    {
                        services.TryAddEnumerable(descriptor);
                    }
                    else
                    {
                        services.TryAdd(descriptor);
                    }
                }
            }
        }

        private static List<Type> GetRegistrationTypes(Type implementationType, Type lookupType, List<Type> implementingTypes, Type[] implementationTypeInterfaces)
        {
            List<Type> lookupTypeServices = implementationTypeInterfaces.Where(type => lookupType != type && type != typeof(IDisposable) && !DependencyInterfaces.Contains(type)).ToList();

            if (!lookupTypeServices.Any())
            {
                if (!DependencyInterfaces.Contains(lookupType))
                {
                    lookupTypeServices.Add(implementingTypes.Count > 1 ? implementationType : lookupType);
                }
                else
                {
                    lookupTypeServices.Add(implementationType);
                }
            }

            return lookupTypeServices;
        }

        private static DependencyType GetDependencyTypeFromType(Type dependencyInterfaceType, DependencyType fallbackDependencyType)
        {
            if (dependencyInterfaceType == typeof(ITransientDependency))
                return DependencyType.Transient;
            if (dependencyInterfaceType == typeof(ISingletonDependency))
                return DependencyType.Singleton;
            if (dependencyInterfaceType == typeof(IScopedDependency))
                return DependencyType.Scoped;
            if (dependencyInterfaceType == typeof(IHostedDependency))
                return DependencyType.Hosted;

            return fallbackDependencyType;
        }

        private static ServiceDescriptor CreateSingletonDescriptor(Type service, Type implementationType)
        {
            return implementationType != null ? ServiceDescriptor.Singleton(service, implementationType) : ServiceDescriptor.Singleton(service);
        }

        private static ServiceDescriptor CreateTransientDescriptor(Type service, Type implementationType)
        {
            return implementationType != null ? ServiceDescriptor.Transient(service, implementationType) : ServiceDescriptor.Singleton(service);
        }

        private static ServiceDescriptor CreateScopedDescriptor(Type service, Type implementationType)
        {
            return implementationType != null ? ServiceDescriptor.Scoped(service, implementationType) : ServiceDescriptor.Singleton(service);
        }

        private static List<Type> GetAllTypesImplementing(this Assembly assembly, Type lookupType)
        {
            return assembly.GetTypes().Where(s => !s.IsAbstract && !s.IsInterface && lookupType.IsAssignableFrom(s)).ToList();
        }

        public static T GetRequiredServiceOrNull<T>(this IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            try
            {
                return (T)provider.GetRequiredService(typeof(T));
            }
            catch
            {
                return default;
            }
        }
    }
}
