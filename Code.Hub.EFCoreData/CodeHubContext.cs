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
        // public DbSet<CodeHubSetting> CodeHubSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Organization>().HasMany(s => s.Projects).WithOne(s => s.Organization).HasForeignKey(s => s.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Project>().HasMany(s => s.Epics).WithOne(s => s.Project).HasForeignKey(s => s.ProjectId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Epic>().HasMany(s => s.WorkLogs).WithOne(s => s.Epic).HasForeignKey(s => s.EpicId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<WorkLog>().HasOne(p => p.Epic).WithMany(i => i.WorkLogs).HasForeignKey(s => s.EpicId).IsRequired(false);

            //modelBuilder.Entity<CodeHubSetting>(builder =>
            //{
            //    builder.HasKey(s => s.Id);
            //    builder.HasIndex(s => s.Key).IsUnique();
            //    builder.Property(s => s.Value).HasJsonConversion();
            //});
        }
    }
}
