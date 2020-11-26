using Code.Hub.Shared.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Code.Hub.EFCoreData
{
    public class CodeHubContextFactory : IDesignTimeDbContextFactory<CodeHubContext>
    {
        public CodeHubContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CodeHubContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            DbContextOptionsConfigurer.Configure(builder, configuration.GetConnectionString("DefaultConnection"));

            return new CodeHubContext(builder.Options);
        }
    }
}