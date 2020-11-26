using Code.Hub.App.Pages.Modal;
using Code.Hub.App.Pages.Modal.ModalTemplates;
using Code.Hub.App.Pages.Toast;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Models;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.Projects
{
    public partial class ProjectList
    {
        private List<Project> Projects { get; set; } = new List<Project>();
        private List<Organization> Organizations { get; set; } = new List<Organization>();
        private GetProjectsInput Input { get; } = new GetProjectsInput();

        protected override async Task OnInitializedAsync()
        {
            if (!(await UserService.IsUserAdmin()))
                UriHelper.NavigateTo(@"\worklogs");

            Input.IncludeOrganizations = true;
            Input.IncludeDisabled = true;
            Organizations = await OrganizationsService.GetOrganizations();
            await Refresh();
        }

        private void OnOrganizationChange(ChangeEventArgs e)
        {
            Input.OrganizationId = int.Parse(e.Value.ToString() ?? throw new InvalidOperationException("Invalid Organization ID Provided"));
            Refresh().GetAwaiter();
        }

        public async Task Refresh()
        {
            Projects = await ProjectsService.GetProjects(Input);
            StateHasChanged();
        }

        public async void Delete(Project project)
        {

            var parameters = new ModalParameters();
            parameters.Add("MessageInfo", $"Please confirm that you want to delete following project: {project.Name}");
            parameters.Add("Id", project.Id);
            Modal.Show("Confirm Delete", typeof(ConfirmModal), parameters);

            Modal.OnClose += DeleteInternal;

            await Task.Yield();
        }

        private async void DeleteInternal(ModalResult modalResult)
        {
            if (!modalResult.Cancelled)
            {
                var isDeleted = await ProjectsService.DeleteProject((int)modalResult.Data);
                if (isDeleted)
                {
                    await Refresh();
                    StateHasChanged();
                }
                else
                    ToastService.ShowToast("Unable to delete! Epics exists on this Project!", ToastLevel.Error);
            }

            Modal.OnClose -= DeleteInternal;
        }

        public void Create(int id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", id);

            Modal.Show("Create Project", typeof(CreateOrEditProject), parameters);
            Modal.OnClose += ModalClosed;
        }


        public void Edit(int id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", id);

            Modal.Show("Edit Project", typeof(CreateOrEditProject), parameters);
            Modal.OnClose += ModalClosed;
        }

        private void ModalClosed(ModalResult modalResult)
        {
            if (!modalResult.Cancelled)
            {
                Console.WriteLine(modalResult.Data);
                Refresh().GetAwaiter();
            }

            Modal.OnClose -= ModalClosed;
        }
    }
}
