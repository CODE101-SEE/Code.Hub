using System;

namespace Code.Hub.Shared.Dtos.Inputs
{
    public class GetWorkLogsInput
    {
        public int ProjectId { get; set; }
        public int OrganizationId { get; set; }
        public int EpicId { get; set; }

        public string UserId { get; set; }

        public string DescriptionFilter { get; set; }

        public DateTime SearchStartDate { get; set; }
        public DateTime SearchEndDate { get; set; }

        public bool IncludeProjects { get; set; }
        public bool IncludeOrganizations { get; set; }
        public bool IncludeEpics { get; set; }
    }
}
