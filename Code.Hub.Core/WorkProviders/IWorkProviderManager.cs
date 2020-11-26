using Code.Hub.Shared.WorkProviders;
using System.Threading.Tasks;

namespace Code.Hub.Core.WorkProviders
{
    public interface IWorkProviderManager
    {
        Task<CodeHubWorkItemList> GetAllWorkItemsFromCache(bool clearCache = false);
    }
}