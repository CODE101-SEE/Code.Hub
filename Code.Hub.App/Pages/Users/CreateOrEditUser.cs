using Code.Hub.App.Pages.Modal;
using Code.Hub.Shared.Dtos.Inputs;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.Users
{
    public partial class CreateOrEditUser
    {

        [CascadingParameter] private ModalParameters Parameters { get; set; }

        public CreateOrEditUserInput Input = new CreateOrEditUserInput();

        private bool IsEdit { get; set; }

        private string UnknownException { get; set; }

        protected override async Task OnInitializedAsync()
        {
            UnknownException = "";
            await LoadUser(Parameters.Get<string>("Id"));
        }

        public async Task LoadUser(string id)
        {
            IsEdit = !string.IsNullOrEmpty(id);
            Input = await UserService.GetUserForEdit(id);
            StateHasChanged();
        }

        public async Task SubmitForm()
        {
            UnknownException = "";
            try
            {
                var created = await UserService.CreateOrEditUser(Input);
                if (created)
                    Done();
                else
                    UnknownException = $"Make sure you entered a valid email address, and that password security settings are met!";
            }
            catch (Exception e)
            {
                UnknownException = e.ToString();
            }
        }

        private void Done()
        {
            Modal.Close(ModalResult.Ok($"User {Input.Id} submitted."));
        }

        private void Cancel()
        {
            Modal.Close(ModalResult.Cancel());
        }

    }
}
