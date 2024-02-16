using ConsoleJobScheduler.Core.Domain.Settings.Model;
using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Domain.Settings.Infra;

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
                    b.Property(x => x.CategoryId).HasColumnName("category_id").IsRequired();
                    b.Property(x => x.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
                    b.Property(x => x.Value).HasColumnName("value");
                });
    }
}