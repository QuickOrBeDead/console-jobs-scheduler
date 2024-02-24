using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Domain.History.Infra;

public sealed class HistoryDbContext : DbContext
{
    public DbSet<Model.JobExecutionHistory> Histories { get; set; }

    public HistoryDbContext(DbContextOptions<HistoryDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Model.JobExecutionHistory>(
            b =>
            {
                b.HasKey(x => x.Id);
                b.ToTable("qrtz_job_history");
                b.Property(x => x.Id).HasMaxLength(30).HasColumnName("id").IsRequired();
                b.Property(x => x.InstanceName).HasColumnName("instance_name").IsRequired();
                b.Property(x => x.SchedulerName).HasColumnName("sched_name").IsRequired();
                b.Property(x => x.JobName).HasColumnName("job_name").IsRequired();
                b.Property(x => x.JobGroup).HasColumnName("job_group").IsRequired();
                b.Property(x => x.PackageName).HasColumnName("package_name").IsRequired();
                b.Property(x => x.TriggerName).HasColumnName("trigger_name").IsRequired();
                b.Property(x => x.TriggerGroup).HasColumnName("trigger_group").IsRequired();
                b.Property(x => x.FiredTime).HasColumnName("fired_time").IsRequired();
                b.Property(x => x.ScheduledTime).HasColumnName("sched_time");
                b.Property(x => x.LastSignalTime).HasColumnName("last_signal_time").IsRequired();
                b.Property(x => x.RunTime).HasColumnName("run_time");
                b.Property(x => x.HasError).HasColumnName("has_error").IsRequired();
                b.Property(x => x.ErrorMessage).HasColumnName("error_message");
                b.Property(x => x.ErrorDetails).HasColumnName("error_details");
                b.Property(x => x.Vetoed).HasColumnName("vetoed").IsRequired();
                b.Property(x => x.Completed).HasColumnName("completed").IsRequired();
                b.Property(x => x.NextFireTime).HasColumnName("next_fire_time");
                b.Property(x => x.CronExpressionString).HasColumnName("cron_expression");
            });
    }
}