using Code.Hub.App.Pages.Modal;
using Code.Hub.App.Pages.Modal.ModalTemplates;
using Code.Hub.App.Pages.Toast;
using Code.Hub.Shared.Dtos.Helpers;
using Code.Hub.Shared.Dtos.Inputs;
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
    public partial class WorkLogList
    {
        private List<Epic> Epics { get; set; } = new List<Epic>();
        private List<Organization> Organizations { get; set; } = new List<Organization>();
        private List<Project> Projects { get; set; } = new List<Project>();
        private List<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
        private List<CodeHubWorkItem> WorkItems { get; set; } = new List<CodeHubWorkItem>();

        private GetProjectsInput ProjectsInput { get; set; } = new GetProjectsInput();
        private GetEpicsInput EpicsInput { get; set; } = new GetEpicsInput();
        private GetWorkLogsInput WorkLogsInput { get; set; } = new GetWorkLogsInput();
        private List<CodeHubUser> LoadedUsers { get; set; } = new List<CodeHubUser>();

        public string TotalTime = "00:00";
        public string TotalTimeWeek = "00:00";
        public string TotalTimeMonth = "00:00";
        public string TotalTimeDay = "00:00";

        private bool IsAdmin { get; set; }
        public bool RefreshingFromProviders { get; set; }

        protected override async Task OnInitializedAsync()
        {
            RefreshingFromProviders = true;
            IsAdmin = await UserService.IsUserAdmin();

            ProjectsInput.IncludeDisabled = false;
            EpicsInput.IncludeDisabled = false;

            WorkLogsInput.IncludeOrganizations = true; //Will include everything
            DateTime now = DateTime.Now;
            WorkLogsInput.SearchStartDate = new DateTime(now.Year, now.Month, 1);
            WorkLogsInput.SearchEndDate = WorkLogsInput.SearchStartDate.AddMonths(1).AddDays(-1);
            WorkLogsInput.UserId = "UnAssigned";

            await LoadUsers();
            await RefreshOrganizations();
            await RefreshWorkLogs();
        }

        public bool IsProjectSelected(int id)
        {
            return id == EpicsInput.ProjectId;
        }

        public async Task LoadUsers()
        {
            LoadedUsers = await UserService.GetAllOrderedUsers();
            StateHasChanged();
        }

        public string GetUserName(string userId)
        {
            var user = LoadedUsers.FirstOrDefault(s => s.Id.ToString() == userId);
            return user?.UserName ?? "User not found!";
        }

        public bool IsEpicSelected(int id)
        {
            return id == WorkLogsInput.EpicId;
        }

        private void OnUserChanges(ChangeEventArgs e)
        {
            WorkLogsInput.UserId = e.Value.ToString();
            RefreshWorkLogs().GetAwaiter();
        }

        private void OnOrganizationChanges(ChangeEventArgs e)
        {
            var id = int.Parse(e.Value.ToString() ?? throw new InvalidOperationException("Invalid Organization Id!"));
            ChangeOrganization(id).GetAwaiter();
        }

        public async Task ChangeOrganization(int id)
        {
            ProjectsInput.OrganizationId = id;
            EpicsInput.OrganizationId = id;
            WorkLogsInput.OrganizationId = id;

            EpicsInput.ProjectId = 0;
            WorkLogsInput.ProjectId = 0;
            WorkLogsInput.EpicId = 0;

            await RefreshProjects();
            await RefreshWorkLogs();
        }

        private void OnProjectChanges(ChangeEventArgs e)
        {
            var id = int.Parse(e.Value.ToString() ?? throw new InvalidOperationException("Invalid Project Id"));
            ChangeProject(id).GetAwaiter();
        }

        private async Task ChangeProject(int id)
        {
            EpicsInput.ProjectId = id;
            WorkLogsInput.ProjectId = id;
            WorkLogsInput.EpicId = 0;

            await RefreshEpics();
            await RefreshWorkLogs();
        }

        public void OnEpicChanges(ChangeEventArgs e)
        {
            WorkLogsInput.EpicId = int.Parse(e.Value.ToString() ?? throw new InvalidOperationException("Invalid epic id!"));
            RefreshWorkLogs().GetAwaiter();
        }

        public async Task RefreshOrganizations()
        {
            Organizations = await OrganizationsService.GetOrganizations();
            StateHasChanged();
        }

        public async Task RefreshProjects()
        {
            Projects = await ProjectsService.GetProjects(ProjectsInput);
            StateHasChanged();
        }

        public async Task RefreshEpics()
        {
            Epics = await EpicsService.GetEpics(EpicsInput);
            StateHasChanged();
        }

        public List<WorkLog> GetWorkLogsFromPeriod(List<WorkLog> logs, TimePeriodEnum period)
        {
            var currentDate = DateTime.Now;
            DateTime startTime;
            DateTime endTime;

            switch (period)
            {
                case TimePeriodEnum.Month:
                    startTime = new DateTime(currentDate.Year, currentDate.Month, 1);
                    endTime = WorkLogsInput.SearchStartDate.AddMonths(1).AddDays(-1);
                    break;
                case TimePeriodEnum.Week:
                    var days = currentDate.DayOfWeek - DayOfWeek.Sunday;
                    startTime = currentDate.AddDays(-days);
                    endTime = startTime.AddDays(6);
                    break;
                case TimePeriodEnum.Day:
                    startTime = currentDate;
                    endTime = currentDate;
                    break;
                default:
                    startTime = currentDate;
                    endTime = currentDate;
                    break;
            }

            var logsInTime = logs.Where(log => log.DateStarted >= startTime && log.DateStarted <= endTime).ToList();
            return logsInTime;
        }

        public async Task RefreshWorkLogs()
        {
            WorkLogs = await WorkLogsService.GetWorkLogs(WorkLogsInput);
            TotalTime = DateTimeHelpers.GetFormattedTime(WorkLogs.Sum(s => s.Hours));
            TotalTimeMonth = DateTimeHelpers.GetFormattedTime(GetWorkLogsFromPeriod(WorkLogs, TimePeriodEnum.Month).Sum(s => s.Hours));
            TotalTimeWeek = DateTimeHelpers.GetFormattedTime(GetWorkLogsFromPeriod(WorkLogs, TimePeriodEnum.Week).Sum(s => s.Hours));
            TotalTimeDay = DateTimeHelpers.GetFormattedTime(GetWorkLogsFromPeriod(WorkLogs, TimePeriodEnum.Day).Sum(s => s.Hours));
            StateHasChanged();

            await PreLoadWorkItems();
        }

        private async Task PreLoadWorkItems()
        {
            var items = await WorkLogsService.GetAllAvailableWorkItemsFromWorkProviders(false);
            WorkItems = items.WorkItems;
            RefreshingFromProviders = false;
        }

        public async void Delete(WorkLog workLog)
        {
            var parameters = new ModalParameters();
            parameters.Add("MessageInfo", $"Please confirm that you want to delete following work log: {workLog.Description}. You worked for {workLog.Hours} hours, on {workLog.DateStarted.ToShortDateString()}.");
            parameters.Add("Id", workLog.Id);
            Modal.Show("Confirm Delete", typeof(ConfirmModal), parameters);

            Modal.OnClose += DeleteInternal;

            await Task.Yield();
        }

        private async void DeleteInternal(ModalResult modalResult)
        {
            if (!modalResult.Cancelled)
            {
                await WorkLogsService.DeleteWorkLog((int)modalResult.Data);
                await RefreshWorkLogs();
                StateHasChanged();
            }

            Modal.OnClose -= DeleteInternal;
        }

        public void Create()
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", 0);

            Modal.Show("Add WorkLog", typeof(CreateOrEditWorkLog), parameters);
            Modal.OnClose += ModalClosed;
        }

        public void Edit(int id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", id);

            Modal.Show("Edit WorkLog", typeof(CreateOrEditWorkLog), parameters);
            Modal.OnClose += ModalClosed;
        }

        private void ModalClosed(ModalResult modalResult)
        {
            if (!modalResult.Cancelled)
                RefreshWorkLogs().GetAwaiter();

            Modal.OnClose -= ModalClosed;
        }

        private void Import()
        {
            Modal.Show("Import WorkLog", typeof(ImportWorkLogs));
            Modal.OnClose += ModalClosed;
        }

        private async Task RefreshFromProviders()
        {
            if (RefreshingFromProviders) return;
            RefreshingFromProviders = true;
            var loadedItems = await WorkLogsService.GetAllAvailableWorkItemsFromWorkProviders(true);
            WorkItems = loadedItems.WorkItems;
            RefreshingFromProviders = false;
        }

        private async Task Export()
        {
            await Task.CompletedTask;
            ToastService.ShowToast("Export Currently Disabled", ToastLevel.Error);

            //var fileBytes = await WorkLogsService.GetDataForExport(WorkLogs);
            //await JsInterop.SaveAs($"Work Logs {DateTime.Now.ToLongDateString()}.xlsx", fileBytes);
        }

        private string GetWorkLogOrganization(WorkLog workLog)
        {
            return Organizations.FirstOrDefault(s => s.Id == workLog.OrganizationId)?.Name ?? "Unknown";
        }

        private string GetWorkLogProject(WorkLog workLog)
        {
            if (WorkItems == null || WorkItems.Count == 0)
                return "Loading...";

            if (workLog.ProviderType == StaticWorkProviderTypes.CodeHub)
                return workLog.Epic?.Project?.Name ?? "Not found";
            else if (workLog.ProviderType == StaticWorkProviderTypes.DevOps)
            {
                var codeHubWorkItem = WorkItems.FirstOrDefault(s => s.Id == workLog.TaskId && workLog.OrganizationId == s.OrganizationId);
                return codeHubWorkItem?.Project ?? "Task Deleted";
            }
            else if (workLog.ProviderType == StaticWorkProviderTypes.Zammad)
            {
                return "Support";
            }

            return "Unknown Provider";
        }

        private string GetWorkLogEpic(WorkLog workLog)
        {
            if (WorkItems == null || WorkItems.Count == 0)
                return "Loading...";

            if (workLog.ProviderType == StaticWorkProviderTypes.CodeHub)
                return workLog.Epic?.Name ?? "Not found";
            else if (workLog.ProviderType == StaticWorkProviderTypes.DevOps)
            {
                var codeHubWorkItem = WorkItems.FirstOrDefault(s =>
                    s.Id == workLog.TaskId && workLog.OrganizationId == s.OrganizationId);
                return codeHubWorkItem?.Parent?.Parent.Title ?? "Task Deleted";
            }
            else if (workLog.ProviderType == StaticWorkProviderTypes.Zammad)
            {
                return "Support";
            }
            return "Unknown Provider";
        }

        private string GetWorkLogFeature(WorkLog workLog)
        {
            if (WorkItems == null || WorkItems.Count == 0)
                return "Loading...";

            if (workLog.ProviderType == StaticWorkProviderTypes.CodeHub)
                return "";
            else if (workLog.ProviderType == StaticWorkProviderTypes.DevOps)
            {
                var codeHubWorkItem = WorkItems.FirstOrDefault(s => s.Id == workLog.TaskId && workLog.OrganizationId == s.OrganizationId);
                return codeHubWorkItem?.Parent?.Title ?? "Task Deleted";
            }
            else if (workLog.ProviderType == StaticWorkProviderTypes.Zammad)
            {
                return "Support";
            }

            return "Unknown Provider";
        }
    }
}
