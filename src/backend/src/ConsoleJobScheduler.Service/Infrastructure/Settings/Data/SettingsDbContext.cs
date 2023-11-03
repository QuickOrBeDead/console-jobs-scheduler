namespace ConsoleJobScheduler.Service.Infrastructure.Settings.Data;

using ConsoleJobScheduler.Service.Infrastructure.Settings.Models;
using Microsoft.EntityFrameworkCore;

public sealed class SettingsDbContext : DbContext
{
    public DbSet<SettingCategoryModel> SettingCategories { get; set; } = default!;
    
    public DbSet<SettingModel> Settings { get; set; } = default!;

    public SettingsDbContext(DbContextOptions<SettingsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SettingCategoryModel>(
            b =>
                {
                    b.HasKey(x => x.Id);
                    b.ToTable("qrtz_settings_category");
                    b.Property(x => x.Name).HasMaxLength(255).IsRequired();
                });

        modelBuilder.Entity<SettingModel>(
            b =>
                {
                    b.HasKey(x => x.Id);
                    b.HasIndex(x => new { x.CategoryId, x.Name }).HasDatabaseName("QX_qrtz_settings_category_id_name").IsUnique();
                    b.ToTable("qrtz_settings");
                    b.Property(x => x.Name).HasMaxLength(255).IsRequired();
                    b.Property(x => x.Value);

                    b.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).IsRequired();
                });
    }
}