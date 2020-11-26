using System.Collections.Generic;

namespace Code.Hub.Shared.WorkProviders
{
    public class CodeHubWorkItemList
    {
        public CodeHubWorkItemList()
        {
            WorkItems = new List<CodeHubWorkItem>();
        }

        public List<CodeHubWorkItem> WorkItems { get; set; }
    }
}
