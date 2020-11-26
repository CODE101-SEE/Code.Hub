using System;
using System.IO;
using System.Linq;

namespace Code.Hub.Shared.Configurations
{
    public static class WebContentDirectoryFinder
    {
        public static string CalculateContentRootFolder()
        {
            var coreAssemblyDirectoryPath = Path.GetDirectoryName(AppContext.BaseDirectory);
            if (coreAssemblyDirectoryPath == null)
            {
                throw new Exception("Could not find location of Code.Hub.Core assembly!");
            }

            var directoryInfo = new DirectoryInfo(coreAssemblyDirectoryPath);
            while (!DirectoryContains(directoryInfo.FullName, "Code.Hub.sln"))
            {
                directoryInfo = directoryInfo.Parent ?? throw new Exception("Could not find content root folder!");
            }

            return Path.Combine(directoryInfo.FullName, $"Code.Hub.App");
        }

        private static bool DirectoryContains(string directory, string fileName)
        {
            return Directory.GetFiles(directory).Any(filePath => string.Equals(Path.GetFileName(filePath), fileName));
        }
    }
}
