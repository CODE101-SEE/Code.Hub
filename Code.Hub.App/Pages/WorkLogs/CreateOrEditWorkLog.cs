using Code.Hub.App.Pages.Modal;
using Code.Hub.Shared.Extensions;
using Code.Hub.Shared.Models;
using Code.Hub.Shared.WorkProviders;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.WorkLogs
{
    public partial class CreateOrEditWorkLog
    {
        [CascadingParameter] private ModalParameters Parameters { get; set; }

        private List<CodeHubWorkItem> WorkItems { get; set; } = new List<CodeHubWorkItem>();
        private List<Organization> Organizations { get; set; } = new List<Organization>();
        private Organization SelectedOrganization { get; set; } = new Organization();
        private Project SelectedProject { get; set; } = new Project();

        public WorkLog Entity = new WorkLog();
        public CodeHubWorkItem SelectedTask { get; set; }
        public string ExceptionMessage = "";
        public string WorkedDateHelper = "00:00";
        public bool InvalidHours;
        public bool CodeHubMode;


        public bool IsOrganizationSelected(int id) => SelectedOrganization.Id == id;
        public bool IsProjectSelected(int id) => SelectedProject.Id == id;
        public bool IsEpicSelected(int id) => Entity.Id == id;

        protected override async Task OnInitializedAsync()
        {
            await LoadWorkItems();
            await LoadWorkLog(Parameters.Get<int>("Id"));
        }

        public async Task LoadOrganizations()
        {
            Organizations = await OrganizationsService.GetOrganizations();
            StateHasChanged();

            var found = Organizations.Find(s => s.Name.Equals("CODE101", StringComparison.InvariantCultureIgnoreCase));
            if (found != null)
            {
                await OrganizationChanged(found.Id);
                StateHasChanged();
            }
        }

        public void OnOrganizationChange(ChangeEventArgs e)
        {
            var newId = int.Parse(e.Value.ToString() ?? throw new InvalidOperationException("Invalid Organization Id"));
            OrganizationChanged(newId).GetAwaiter();
        }

        public void OnProjectChanges(ChangeEventArgs e)
        {
            var newId = int.Parse(e.Value.ToString() ?? throw new InvalidOperationException("Invalid Project Id"));
            ProjectChanged(newId);
        }

        public async Task OrganizationChanged(int id)
        {
            SelectedProject = new Project();
            Entity.Epic = new Epic();
            Entity.EpicId = 0;

            SelectedOrganization = await OrganizationsService.GetOrganization(id);

            StateHasChanged();
        }

        public void ProjectChanged(int id)
        {
            Entity.Epic = new Epic();
            Entity.EpicId = 0;

            SelectedProject = SelectedOrganization.Projects.FirstOrDefault(s => s.Id == id);
            StateHasChanged();
        }

        public string GetWorkItemClass(CodeHubWorkItem item)
        {
            return $"{item.ProviderType} {item.ProviderType}-{item.WorkItemType.Replace(" ", "")}";
        }

        private List<CodeHubWorkItem> GetCodeHubWorkItems()
        {
            return new List<CodeHubWorkItem> { new CodeHubWorkItem { Id = 0, Title = "Generic", ProviderType = StaticWorkProviderTypes.CodeHub, WorkItemType = "CodeHub" } };

        }

        private async Task<IEnumerable<CodeHubWorkItem>> SearchWorkItems(string searchText)
        {
            if (CodeHubMode)
                return GetCodeHubWorkItems();

            searchText ??= "".ToLower();

            return await Task.FromResult(WorkItems.Where(item => item.Id.ToString().Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
                                                                 || item.Title != null && item.Title.ToLower().Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
                                                                 || item.AssignedTo != null && item.AssignedTo.ToLower().Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
                                                                 || item.WorkItemType != null && item.WorkItemType.ToLower().Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
                                                                 || item.Project != null && item.Project.ToLower().Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
                                                                 || item.State != null && item.State.ToLower().Contains(searchText, StringComparison.InvariantCultureIgnoreCase)
                                                                 || (item.ProviderType == StaticWorkProviderTypes.DevOps && item.Parent != null && item.Parent.Title.Contains(searchText, StringComparison.InvariantCultureIgnoreCase))
                                                                 || (item.ProviderType == StaticWorkProviderTypes.DevOps && item.Parent?.Parent != null && item.Parent.Parent.Title.Contains(searchText, StringComparison.InvariantCultureIgnoreCase))
            ).OrderByDescending(s => s.ChangedDate).ToList());
        }

        private async Task LoadWorkItems()
        {
            var items = await WorkLogsService.GetAllAvailableWorkItemsFromWorkProviders(false);
            WorkItems = items.WorkItems;
        }

        public async Task LoadWorkLog(int id)
        {
            DisableCodeHubMode();

            var found = await WorkLogsService.GetWorkLogForEdit(id);

            if (found.TaskId > 0 && found.ProviderType == StaticWorkProviderTypes.DevOps || found.ProviderType == StaticWorkProviderTypes.Zammad)
                SelectedTask = WorkItems.FirstOrDefault(s => s.Id == found.TaskId);
            else if (found.Id > 0 && found.ProviderType == StaticWorkProviderTypes.CodeHub)
            {
                await EnableCodeHubMode();
                if (found.Epic != null)
                {
                    await OrganizationChanged(found.Epic.Project.OrganizationId);
                    ProjectChanged(found.Epic.ProjectId);
                }
            }

            Entity = found;
            WorkedDateHelper = DateTimeHelpers.GetFormattedTime(Entity.Hours);

            StateHasChanged();
        }

        private async Task EnableCodeHubMode()
        {
            CodeHubMode = true;
            SelectedTask = GetCodeHubWorkItems().FirstOrDefault() ?? new CodeHubWorkItem();

            await LoadOrganizations();
        }

        private void DisableCodeHubMode()
        {
            Entity.EpicId = null;
            Entity.Epic = null;
            SelectedProject = new Project();
            CodeHubMode = false;
        }

        public bool IsDisabled()
        {
            if (WorkedDateHelper == "00:00") return true;
            if (CodeHubMode) return (!Entity.EpicId.HasValue);
            if (!CodeHubMode) return (SelectedTask == null || SelectedTask.Id == 0);

            return true;
        }

        public async Task SubmitForm()
        {
            InvalidHours = false;
            ExceptionMessage = "";
            try
            {
                Entity.Hours = DateTimeHelpers.GetHoursFromString(WorkedDateHelper, true);
                try
                {
                    Entity.ProviderType = CodeHubMode ? StaticWorkProviderTypes.CodeHub : SelectedTask.ProviderType;
                    Entity.OrganizationId = CodeHubMode ? SelectedOrganization.Id : SelectedTask.OrganizationId;
                    Entity.TaskId = CodeHubMode ? 0 : SelectedTask.Id; // TODO CodeHub Tasks
                    Entity = await WorkLogsService.CreateOrEditWorkLog(Entity);
                    Done();
                }
                catch (Exception e)
                {
                    ExceptionMessage = e.Message;
                }
            }
            catch
            {
                InvalidHours = true;
                Entity.Hours = 0;
            }

            StateHasChanged();
        }

        private void Done()
        {
            Modal.Close(ModalResult.Ok($"WorkLog {Entity?.Id} submitted."));
        }

        private void Cancel()
        {
            Modal.Close(ModalResult.Cancel());
        }

        private async Task SwitchMode()
        {
            SelectedTask = new CodeHubWorkItem();
            if (CodeHubMode)
            {
                DisableCodeHubMode();
            }
            else
                await EnableCodeHubMode();
        }
    }
}
