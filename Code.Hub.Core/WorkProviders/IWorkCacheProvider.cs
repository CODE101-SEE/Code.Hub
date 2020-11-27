using System.Threading.Tasks;
using Code.Hub.Shared.Models;

namespace Code.Hub.Core.WorkProviders
{
    public interface IWorkCacheProvider
    {
        ValueTask InvalidateAllCacheAsync();
        ValueTask InvalidateCacheAsync(Organization organization);
    }
}
