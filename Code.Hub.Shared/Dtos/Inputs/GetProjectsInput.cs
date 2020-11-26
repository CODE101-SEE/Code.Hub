namespace Code.Hub.Shared.Dtos.Inputs
{
    public class GetProjectsInput
    {
        public int OrganizationId { get; set; }
        public string NameFilter { get; set; }
        public bool IncludeOrganizations { get; set; }
        public bool IncludeDisabled { get; set; }
    }
}
