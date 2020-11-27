using System;
using Code.Hub.Shared.WorkProviders;
using Microsoft.Extensions.Options;

namespace Code.Hub.Core.WorkProviders
{
    public class WorkProviderFactory : IWorkProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<WorkProviderOptions> _options;

        public WorkProviderFactory(IServiceProvider serviceProvider, IOptions<WorkProviderOptions> options)
        {
            _serviceProvider = serviceProvider;
            _options = options;
        }

        public IWorkProvider GetWorkProvider(WorkProviderType providerType)
        {
            if (_options.Value.Mappings.TryGetValue(providerType, out Type implementationType))
                return _serviceProvider.GetService(implementationType) as IWorkProvider;
            return null;
        }
    }
}