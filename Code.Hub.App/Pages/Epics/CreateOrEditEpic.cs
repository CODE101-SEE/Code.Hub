using Code.Hub.App.Pages.Modal;
using Code.Hub.Shared.Dtos.Inputs;
using Code.Hub.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.Epics
{
    public partial class CreateOrEditEpic
    {
        [CascadingParameter] protected ModalParameters Parameters { get; set; }

        public Organization SelectedOrganization { get; set; } = new Organization();
        public List<Organization> Organizations { get; set; }

        public List<Project> Projects { get; set; } = new List<Project>();
        public Epic EpicEntity { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadOrganizations();
            await LoadEpic(Parameters.Get<int>("Id"));
        }

        public bool IsOrganizationSelected(int id)
        {
            return SelectedOrganization.Id == id;
        }

        public async Task LoadOrganizations()
        {
            Organizations = await OrganizationsService.GetOrganizations();
            StateHasChanged();
        }

        public async Task TriggerSelectOrganization(ChangeEventArgs e)
        {
            await SelectOrganization(int.Parse(e.Value.ToString() ?? string.Empty));
        }

        public async Task SelectOrganization(int id)
        {
            SelectedOrganization = await OrganizationsService.GetOrganization(id);

            var input = new GetProjectsInput { IncludeOrganizations = true, OrganizationId = id, IncludeDisabled = false };
            Projects = await ProjectsService.GetProjects(input);

            StateHasChanged();
        }

        public async Task LoadEpic(int id)
        {
            EpicEntity = await EpicsService.GetEpicOrNull(id);
            if (EpicEntity.Id != 0)
                await SelectOrganization(EpicEntity.Project.OrganizationId);
        }

        public async Task SubmitForm()
        {
            if (OrganizationsService.OrganizationExists(SelectedOrganization.Id) && ProjectsService.ProjectExists(EpicEntity.ProjectId))
            {
                EpicEntity.Project = await ProjectsService.GetProjectOrNull(EpicEntity.ProjectId);
                await SaveOrganization();
            }
        }

        public async Task SaveOrganization()
        {
            EpicEntity = await EpicsService.CreateOrEdit(EpicEntity);
            Done();
        }

        public void Done()
        {
            Modal.Close(ModalResult.Ok($"{EpicEntity.Id} submitted"));
        }

        public void Cancel()
        {
            Modal.Close(ModalResult.Cancel());
        }
    }
}
