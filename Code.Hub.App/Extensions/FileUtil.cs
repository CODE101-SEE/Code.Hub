using Code.Hub.Shared.Dtos.Helpers;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Code.Hub.App.Extensions
{
    public static class FileUtil
    {
        public static ValueTask<object> SaveAs(this IJSRuntime js, string filename, byte[] data)
        {
            return js.InvokeAsync<object>(
                           "saveAsFile",
                           filename,
                           Convert.ToBase64String(data));
        }

        public static async Task<string> GetFileData(this IJSRuntime js, string fileInputRef)
        {
            try
            {
                var res = await js.InvokeAsync<StringHolder>("getFileData", TimeSpan.FromSeconds(60), fileInputRef);
                return res.Content;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
