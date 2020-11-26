using Microsoft.AspNetCore.Components;

namespace Code.Hub.App.Pages.Modal.ModalTemplates
{
    public partial class ConfirmModal : ComponentBase
    {
        [CascadingParameter] ModalParameters Parameters { get; set; }

        public string Message { get; set; }
        public int Id { get; set; }

        protected override void OnInitialized()
        {
            Message = Parameters.Get<string>("MessageInfo");
            Id = Parameters.Get<int>("Id");
        }

        void Done()
        {
            Modal.Close(ModalResult.Ok(Id));
        }

        void Cancel()
        {
            Modal.Close(ModalResult.Cancel());
        }
    }
}
