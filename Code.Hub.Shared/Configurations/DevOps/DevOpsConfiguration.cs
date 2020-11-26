namespace Code.Hub.Shared.Configurations.DevOps
{
    public class DevOpsConfiguration
    {
        public int CacheDuration { get; set; }
        public string OrganizationUrl { get; set; }
        public string OrganizationOwnerPatToken { get; set; }

        public string WorkItemQuery { get; set; }
        public string RequiredWorkItemFieldString { get; set; }
        public string OrderByWorkItemFieldName { get; set; }
        public string AllWorkItemsQuery { get; set; }

        public string[] GetWorkItemFields()
        {
            if (string.IsNullOrWhiteSpace(RequiredWorkItemFieldString)) return new string[] { };

            return RequiredWorkItemFieldString.Split(';');
        }
    }
}
