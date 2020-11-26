using Code.Hub.App.Pages.Modal;
using Code.Hub.Shared.Models;
using Code.Hub.Shared.WorkProviders;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.Organizations
{
    public partial class CreateOrEditOrganization
    {
        [CascadingParameter] private ModalParameters Parameters { get; set; }

        public Organization OrganizationInfo { get; set; } = new Organization();
        // Add only after support was tested completely 
        public string[] AvailableOrganizationTypes = { StaticWorkProviderTypes.CodeHub, StaticWorkProviderTypes.DevOps, StaticWorkProviderTypes.Zammad };

        public bool IsProviderSelected(string type) => OrganizationInfo.ProviderType == type;

        protected override void OnInitialized()
        {
            LoadOrganization(Parameters.Get<int>("Id")).GetAwaiter();
        }

        public async Task LoadOrganization(int id)
        {
            OrganizationInfo = await OrganizationsService.GetOrganizationForEdit(id);
            StateHasChanged();
        }

        public async Task SubmitForm()
        {
            await SaveOrganization();
        }

        public async Task SaveOrganization()
        {
            await OrganizationsService.CreateOrEditOrganization(OrganizationInfo);
            Done();
        }

        public void Done()
        {
            Modal.Close(ModalResult.Ok($"Organization {OrganizationInfo.Id} submitted."));
        }

        public void Cancel()
        {
            Modal.Close(ModalResult.Cancel());
        }
    }
}
