using System;

namespace Code.Hub.Shared.WorkProviders
{
    public class CodeHubWorkItem
    {
        public int Id { get; set; }
        public string ProviderType { get; set; }
        public string ProviderOrganization { get; set; }
        public int OrganizationId { get; set; }

        public string Title { get; set; }
        public string WorkItemType { get; set; }
        public string AssignedTo { get; set; }
        public string Project { get; set; }
        public string State { get; set; }
        public string Color { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ChangedDate { get; set; }

        public CodeHubWorkItem Parent { get; set; }
    }
}