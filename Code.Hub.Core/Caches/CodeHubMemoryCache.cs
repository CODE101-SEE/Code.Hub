using Microsoft.Extensions.Caching.Memory;

namespace Code.Hub.Core.Caches
{
    public class CodeHubMemoryCache : ICodeHubCache
    {
        public CodeHubMemoryCache()
        {
            Cache = new MemoryCache(new MemoryCacheOptions());
        }

        public MemoryCache Cache { get; }
    }
}
