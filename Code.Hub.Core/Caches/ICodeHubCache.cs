using Code.Hub.Core.Dependency;
using Microsoft.Extensions.Caching.Memory;

namespace Code.Hub.Core.Caches
{
    public interface ICodeHubCache : ISingletonDependency
    {
        public MemoryCache Cache { get; }
    }
}