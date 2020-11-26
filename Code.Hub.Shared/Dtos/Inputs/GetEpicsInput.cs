namespace Code.Hub.Shared.Dtos.Inputs
{
    public class GetEpicsInput
    {
        public int ProjectId { get; set; }
        public int OrganizationId { get; set; }
        public string NameFilter { get; set; }
        public bool IncludeProjects { get; set; }
        public bool IncludeOrganizations { get; set; }
        public bool IncludeDisabled { get; set; }
    }
}
