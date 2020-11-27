using System;
using System.Collections.Generic;
using Code.Hub.Shared.WorkProviders;

namespace Code.Hub.Core.WorkProviders
{
    public class WorkProviderOptions
    {
        private readonly Dictionary<WorkProviderType, Type> _mappings = new Dictionary<WorkProviderType, Type>();

        public IReadOnlyDictionary<WorkProviderType, Type> Mappings => _mappings;

        /// <summary>
        /// Maps WorkProvider implementation to Provider type
        /// </summary>
        /// <typeparam name="T">IWorkerRestrictionEvaluateProvider implementation</typeparam>
        public virtual void AddMapping<T>(WorkProviderType provider) where T : IWorkProvider
        {
            AddMapping(typeof(T), provider);
        }

        private void AddMapping(Type providerType, WorkProviderType provider)
        {
            if (!_mappings.ContainsKey(provider))
                 _mappings.Add(provider, providerType);
        }
    }
}
