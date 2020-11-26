using Code.Hub.App.Pages.Modal;
using Code.Hub.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.Projects
{
    public partial class CreateOrEditProject
    {

        [CascadingParameter] private ModalParameters Parameters { get; set; }

        private Project ProjectEntity { get; set; } = new Project();

        private List<Organization> Organizations { get; set; } = new List<Organization>();

        protected override void OnInitialized()
        {
            LoadOrganizations().GetAwaiter();
            LoadProject(Parameters.Get<int>("Id")).GetAwaiter();
        }

        public bool IsSelected(int id)
        {
            return id == ProjectEntity.OrganizationId;
        }

        public async Task LoadOrganizations()
        {
            Organizations = await OrganizationsService.GetOrganizations();
            StateHasChanged();
        }

        public async Task LoadProject(int id)
        {
            ProjectEntity = await ProjectsService.GetProjectForEdit(id);
            StateHasChanged();
        }

        public async Task SubmitForm()
        {
            ProjectEntity = (ProjectEntity.Id == 0) ? await ProjectsService.CreateProject(ProjectEntity) : await ProjectsService.UpdateProject(ProjectEntity);
            Done();
        }

        private void Done()
        {
            Modal.Close(ModalResult.Ok($"Project {ProjectEntity.Id} submitted."));
        }

        private void Cancel()
        {
            Modal.Close(ModalResult.Cancel());
        }
    }
}
