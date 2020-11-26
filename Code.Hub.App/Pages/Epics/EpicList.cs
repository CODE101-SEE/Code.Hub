using Code.Hub.App.Pages.Modal;
using Code.Hub.App.Pages.Modal.ModalTemplates;
using Code.Hub.App.Pages.Toast;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.Epics
{
    public partial class EpicList
    {
        public List<Epic> Epics { get; set; } = new List<Epic>();
        public List<Organization> Organizations = new List<Organization>();
        public List<Project> Projects = new List<Project>();

        public GetProjectsInput GetProjectsInput = new GetProjectsInput();
        public GetEpicsInput EpicsInput = new GetEpicsInput();

        protected override async Task OnInitializedAsync()
        {
            if (!(await UserService.IsUserAdmin()))
                UriHelper.NavigateTo(@"\worklogs");

            EpicsInput.IncludeOrganizations = true;
            EpicsInput.IncludeDisabled = true;
            GetProjectsInput.IncludeDisabled = false;
            Organizations = await OrganizationsService.GetOrganizations();

            await Refresh();
        }


        public bool IsProjectSelected(int id)
        {
            return id == EpicsInput.ProjectId;
        }

        private void OnOrganizationChanges(ChangeEventArgs e)
        {
            var id = int.Parse(e.Value.ToString() ?? throw new Exception("Invalid organization id"));
            ChangeOrganization(id).GetAwaiter();
        }

        private async Task ChangeOrganization(int id)
        {
            GetProjectsInput.OrganizationId = id;
            await RefreshProjects();

            EpicsInput.OrganizationId = id;
            EpicsInput.ProjectId = 0;
            await Refresh();
        }

        private void TriggerProjectChanges(ChangeEventArgs e)
        {
            var id = int.Parse(e.Value.ToString() ?? throw new InvalidOperationException("Invalid project id"));

            EpicsInput.ProjectId = id;
            Refresh().GetAwaiter();
        }

        public async Task RefreshProjects()
        {
            Projects = (GetProjectsInput.OrganizationId == 0) ? new List<Project>() : await ProjectsService.GetProjects(GetProjectsInput);
            this.StateHasChanged();
        }

        public async Task Refresh()
        {
            Epics = await EpicsService.GetEpics(EpicsInput);
            StateHasChanged();
        }

        public async void Delete(Epic epic)
        {

            var parameters = new ModalParameters();
            parameters.Add("MessageInfo", $"Please confirm that you want to delete following epic: {epic.Name}");
            parameters.Add("Id", epic.Id);
            Modal.Show("Confirm Delete", typeof(ConfirmModal), parameters);

            Modal.OnClose += DeleteInternal;

            await Task.Yield();
        }

        private async void DeleteInternal(ModalResult modalResult)
        {
            if (!modalResult.Cancelled)
            {
                var isDeleted = await EpicsService.DeleteEpic((int)modalResult.Data);
                if (isDeleted)
                {
                    await Refresh();
                    StateHasChanged();
                }
                else
                {
                    ToastService.ShowToast("Unable to delete! Work logs exists on this epic!", ToastLevel.Error);
                }
            }

            Modal.OnClose -= DeleteInternal;
        }

        public void Create()
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", 0);

            Modal.Show("Create Epic", typeof(CreateOrEditEpic), parameters);
            Modal.OnClose += ModalClosed;
        }

        public void Edit(int id)
        {

            var parameters = new ModalParameters();
            parameters.Add("Id", id);

            Modal.Show("Edit Epic", typeof(CreateOrEditEpic), parameters);
            Modal.OnClose += ModalClosed;
        }

        private void ModalClosed(ModalResult modalResult)
        {
            if (!modalResult.Cancelled)
                Refresh().GetAwaiter();

            Modal.OnClose -= ModalClosed;
        }
    }
}
