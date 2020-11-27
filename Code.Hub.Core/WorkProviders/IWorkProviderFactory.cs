using Code.Hub.Shared.WorkProviders;

namespace Code.Hub.Core.WorkProviders
{
    public interface IWorkProviderFactory
    {
        IWorkProvider GetWorkProvider(WorkProviderType providerType);
    }
}
