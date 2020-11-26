using Code.Hub.App.Pages.Modal;
using Code.Hub.App.Pages.Toast;
using Code.Hub.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.Users
{
    public partial class UsersList
    {
        private List<CodeHubUser> Users { get; set; } = new List<CodeHubUser>();

        protected override async Task OnInitializedAsync()
        {
            if (!(await UserService.IsUserAdmin()))
                UriHelper.NavigateTo(@"\worklogs");

            await Refresh();
        }

        public async Task Refresh()
        {
            await Task.Yield();
            Users = await UserService.GetAllOrderedUsers();
            StateHasChanged();
        }

        public async void Delete(CodeHubUser user)
        {
            ToastService.ShowToast("Deleting users is currently disabled!", ToastLevel.Warning);

            await Task.Yield();
        }

        public void Create()
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", "");

            Modal.Show("Create User", typeof(CreateOrEditUser), parameters);
            Modal.OnClose += ModalClosed;
        }


        public void Edit(string id)
        {
            var parameters = new ModalParameters();
            parameters.Add("Id", id);

            Modal.Show("Edit User", typeof(CreateOrEditUser), parameters);
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
