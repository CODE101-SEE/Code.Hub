using Microsoft.EntityFrameworkCore;

namespace Code.Hub.EFCoreData
{
    public static class DbContextOptionsConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<CodeHubContext> dbContextOptions, string connectionString)
        {
            /* This is the single point to configure DbContextOptions for testDbContext */
            dbContextOptions.UseSqlServer(connectionString);
        }
    }
}