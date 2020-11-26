using Code.Hub.App.Pages.Toast;
using Code.Hub.Shared.WorkProviders;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.DevOps
{
    public partial class DevOpsList : ComponentBase
    {
        private bool IsAdmin { get; set; }
        public bool ProcessingDevOpsFix { get; set; }
        public List<CodeHubWorkItem> WorkItems { get; set; }
        public List<CodeHubWorkItemTree> TreeNodes { get; set; }

        protected override async Task OnInitializedAsync()
        {
            IsAdmin = await UserService.IsUserAdmin();
            if (!IsAdmin)
                UriHelper.NavigateTo(@"\worklogs");

            await GetWorkItems();
        }

        public async Task GetWorkItems()
        {
            var items = await DevOpsManager.GetAllWorkItemsFromCache(false);
            WorkItems = items.WorkItems;
            await BuildTree();
        }

        public async Task BuildTree()
        {
            TreeNodes = new List<CodeHubWorkItemTree>();

            var features = WorkItems.Select(s => s.Parent).Distinct().ToList();

            foreach (var feature in features.OrderBy(s => s.ProviderOrganization).ThenBy(s => s.Project).ThenBy(s => s.Parent.Id))
            {
                var childIds = WorkItems.Where(s => s.Parent.Id == feature.Id).Select(s => (long)s.Id).ToList();
                childIds.Add(feature.Id);
                var hoursWorked = await WorkLogsService.GetHoursWorked(childIds);
                var featureNode = new CodeHubWorkItemTree
                {
                    Children = new List<CodeHubWorkItemTree>(),
                    Id = feature.Id,
                    Title = feature.Title,
                    ProviderOrganization = feature.ProviderOrganization,
                    Project = feature.Project,
                    NodeLevel = feature.WorkItemType,
                    ChildCount = childIds.Count(),
                    Parent = feature.Parent,
                    HoursWorked = hoursWorked
                };

                TreeNodes.Add(featureNode);
            }

            StateHasChanged();
        }

        private async Task FixDevOpsIds()
        {
            if (ProcessingDevOpsFix) return;
            ProcessingDevOpsFix = true;
            var message = await WorkLogsService.FixDevOpsIds();
            ToastService.ShowToast(message, ToastLevel.Info);
            ProcessingDevOpsFix = false;
        }

    }
}
