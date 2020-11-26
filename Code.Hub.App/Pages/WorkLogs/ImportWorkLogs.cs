using Code.Hub.App.Pages.Modal;
using Code.Hub.App.Pages.Toast;
using System.Threading.Tasks;

namespace Code.Hub.App.Pages.WorkLogs
{
    public partial class ImportWorkLogs
    {
        private async Task StartImport()
        {
            await Task.CompletedTask;
            ToastService.ShowToast("Import Currently Disabled", ToastLevel.Error);
            Done();
            return;

            //var data = await JsInterop.GetFileData("fileUpload");

            //var message = await WorkLogsService.ImportFromExcel(data);
            //ToastService.ShowToast(message, ToastLevel.Info);

            //Done();
        }

        void Done()
        {
            Modal.Close(ModalResult.Ok($"Import started."));
        }

        void Cancel()
        {
            Modal.Close(ModalResult.Cancel());
        }

    }
}
