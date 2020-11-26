using System;
using System.IO;

namespace Code.Hub.Shared.Extensions
{
    public static class FileHelpers
    {
        public static string SaveTempFile(string data, string extension)
        {
            var dataBytes = Convert.FromBase64String(data);

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + $".{extension}");
            File.WriteAllBytes(tempPath, dataBytes);

            return tempPath;
        }

        public static void DeleteFile(string path)
        {
            File.Delete(path);
        }
    }
}
