using Code.Hub.Shared.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Code.Hub.EFCoreData
{
    public class CodeHubContext : IdentityDbContext<CodeHubUser, CodeHubRole, string>
    {
        public CodeHubContext(DbContextOptions<CodeHubContext> options) : base(options)
        { }
        public DbSet<WorkLog> WorkLogs { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Epic> Epics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Organization>(builder =>
            {
                builder.HasKey(sc => new { sc.Id });
                builder.Property(sc => sc.ProviderType).HasConversion<string>();
                builder.HasMany(s => s.Projects).WithOne(s => s.Organization).HasForeignKey(s => s.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Project>(builder =>
            {
                builder.HasKey(sc => new { sc.Id });
                builder.HasMany(s => s.Epics).WithOne(s => s.Project).HasForeignKey(s => s.ProjectId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Epic>(builder =>
            {
                builder.HasKey(sc => new { sc.Id });
                builder.HasMany(s => s.WorkLogs).WithOne(s => s.Epic).HasForeignKey(s => s.EpicId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<WorkLog>(builder =>
            {
                builder.HasKey(sc => new { sc.Id });
                builder.HasOne(s => s.Epic).WithMany(s => s.WorkLogs).HasForeignKey(s => s.EpicId).IsRequired(false);
            });
        }
    }
}
