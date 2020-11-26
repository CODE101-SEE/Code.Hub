using System.Collections.Generic;

namespace Code.Hub.Shared.WorkProviders
{
    public class CodeHubWorkItemTree : CodeHubWorkItem
    {
        public string NodeLevel { get; set; }
        public double HoursWorked { get; set; }
        public int ChildCount { get; set; }

        public List<CodeHubWorkItemTree> Children { get; set; }
    }
}
