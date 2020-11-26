using Code.Hub.App.Pages.Modal;
using Code.Hub.App.Pages.Modal.ModalTemplates;
using Code.Hub.App.Pages.Toast;
using Code.Hub.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.Organizations
{
    public partial class OrganizationList
    {
        private List<Organization> Organizations { get; set; } = new List<Organization>();

        public string GetTextSummary(string fullText, int breakPoint)
        {
            return (fullText.Length < breakPoint) ? fullText : fullText.Substring(0, breakPoint) + "...";
        }

        protected override async Task OnInitializedAsync()
        {
            if (!(await UserService.IsUserAdmin()))
                UriHelper.NavigateTo(@"\worklogs");

            await Refresh();
        }

        public async Task Refresh()
        {
            Organizations = await OrganizationsService.GetOrganizations();
            StateHasChanged();
        }

        public async void Delete(Organization organization)
        {

            var parameters = new ModalParameters();
            parameters.Add("MessageInfo", $"Please confirm that you want to delete following organization: {organization.Name}");
            parameters.Add("Id", organization.Id);
            Modal.Show("Confirm Delete", typeof(ConfirmModal), parameters);

            Modal.OnClose += DeleteInternal;

            await Task.Yield();
        }

        private async void DeleteInternal(ModalResult modalResult)
        {
            if (!modalResult.Cancelled)
            {
                var isDeleted = await OrganizationsService.DeleteOrganization((int)modalResult.Data);
                if (isDeleted)
                {
                    StateHasChanged();
                    await Refresh();
                }
                else
                    ToastService.ShowToast("Unable to delete! Projects exists on this Organization!", ToastLevel.Error);
            }

            Modal.OnClose -= DeleteInternal;
        }


        public void Create()
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", 0);

            Modal.Show("Create Organization", typeof(CreateOrEditOrganization), parameters);
            Modal.OnClose += ModalClosed;
        }


        public void Edit(int id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", id);

            Modal.Show("Edit Organization", typeof(CreateOrEditOrganization), parameters);
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
