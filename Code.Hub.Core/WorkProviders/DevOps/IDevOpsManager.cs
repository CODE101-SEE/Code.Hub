using Code.Hub.Core.Dependency;
using Code.Hub.Shared.Configurations.DevOps;
using Code.Hub.Shared.WorkProviders;
using System.Threading.Tasks;

namespace Code.Hub.Core.WorkProviders.DevOps
{
    public interface IDevOpsManager : IWorkProviderManager, IScopedDependency
    {
        Task<CodeHubWorkItemList> GetWorkItemsFromProviderFromCache(DevOpsConfiguration configuration, bool clearCache = false);
        Task<CodeHubWorkItemList> GetWorkItemTreeFromProviderFromCache(DevOpsConfiguration configuration, bool clearCache = false);
    }
}