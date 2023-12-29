using ConsoleJobScheduler.Core.Infrastructure.Settings.Models;

using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Infrastructure.Settings.Data;

public sealed class SettingsDbContext : DbContext
{
    public DbSet<SettingModel> Settings { get; set; } = null!;

    public SettingsDbContext(DbContextOptions<SettingsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SettingModel>(
            b =>
                {
                    b.HasKey(x => new { x.CategoryId, x.Name });
                    b.ToTable("qrtz_settings");
                    b.Property(x => x.CategoryId).IsRequired();
                    b.Property(x => x.Name).HasMaxLength(255).IsRequired();
                    b.Property(x => x.Value);
                });
    }
}