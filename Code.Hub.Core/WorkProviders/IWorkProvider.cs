using System.Threading.Tasks;
using Code.Hub.Shared.Models;
using Code.Hub.Shared.WorkProviders;

namespace Code.Hub.Core.WorkProviders
{
    public interface IWorkProvider
    {
        Task<CodeHubWorkItemList> GetAllWorkItems();
        Task<CodeHubWorkItemList> GetWorkItems(Organization organization);
    }
}
