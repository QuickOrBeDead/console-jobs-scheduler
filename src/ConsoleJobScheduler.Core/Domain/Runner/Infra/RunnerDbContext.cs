using Microsoft.EntityFrameworkCore;

namespace ConsoleJobScheduler.Core.Domain.Runner.Infra;

public sealed class RunnerDbContext : DbContext
{
    public DbSet<Model.JobPackage> JobPackages { get; set; }

    public DbSet<Model.JobRunLog> JobRunLogs { get; set; }

    public DbSet<Model.JobRunEmail> JobRunEmails { get; set; }

    public DbSet<Model.JobRunAttachment> JobRunAttachments { get; set; }

    public RunnerDbContext(DbContextOptions<RunnerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Model.JobPackage>(
            b =>
            {
                b.HasKey(x => x.Name);
                b.ToTable("qrtz_packages");
                b.Property(x => x.Name).HasMaxLength(256).HasColumnName("name").IsRequired();
                b.Property(x => x.Content).HasColumnName("content").IsRequired();
                b.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(1024).IsRequired();
                b.Property(x => x.Arguments).HasColumnName("arguments").HasMaxLength(1024).IsRequired();
                b.Property(x => x.Author).HasColumnName("author").HasMaxLength(50).IsRequired();
                b.Property(x => x.Description).HasColumnName("description").HasMaxLength(1024).IsRequired();
                b.Property(x => x.Version).HasColumnName("version").HasMaxLength(50).IsRequired();
                b.Property(x => x.CreateDate).HasColumnName("create_time").IsRequired();
            });

        modelBuilder.Entity<Model.JobRunLog>(
            b =>
            {
                b.HasKey(x => x.Id);
                b.ToTable("qrtz_job_run_log");
                b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn().ValueGeneratedOnAdd().IsRequired();
                b.Property(x => x.JobRunId).HasColumnName("job_run_id").HasMaxLength(30).IsRequired();
                b.Property(x => x.Content).HasColumnName("content").HasColumnType("text").IsRequired();
                b.Property(x => x.IsError).HasColumnName("is_error").IsRequired();
                b.Property(x => x.CreateDate).HasColumnName("create_time").IsRequired();

                b.HasIndex(x => x.JobRunId)
                    .HasDatabaseName("idx_qrtz_job_run_log_job_run_id");
            });

        modelBuilder.Entity<Model.JobRunEmail>(
            b =>
            {
                b.HasKey(x => x.Id);
                b.ToTable("qrtz_job_run_email");
                b.Property(x => x.Id).HasColumnName("id").IsRequired();
                b.Property(x => x.JobRunId).HasColumnName("job_run_id").HasMaxLength(30).IsRequired();
                b.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(256).IsRequired();
                b.Property(x => x.Body).HasColumnName("body").HasColumnType("text").IsRequired();
                b.Property(x => x.To).HasColumnName("message_to").HasColumnType("text").IsRequired();
                b.Property(x => x.CC).HasColumnName("message_cc").HasColumnType("text").IsRequired();
                b.Property(x => x.Bcc).HasColumnName("message_bcc").HasColumnType("text").IsRequired();
                b.Property(x => x.IsSent).HasColumnName("is_sent").IsRequired();
                b.Property(x => x.CreateDate).HasColumnName("create_time").IsRequired();

                b
                    .HasMany(e => e.Attachments)
                    .WithOne(x => x.JobRunEmail)
                    .HasForeignKey(e => e.EmailId);

                b.HasIndex(x => x.JobRunId)
                    .HasDatabaseName("idx_qrtz_job_run_email_job_run_id");
            });

        modelBuilder.Entity<Model.JobRunAttachment>(
            b =>
            {
                b.HasKey(x => x.Id);
                b.ToTable("qrtz_job_run_attachment");
                b.Property(x => x.Id).HasColumnName("id").IsRequired();
                b.Property(x => x.JobRunId).HasColumnName("job_run_id").HasMaxLength(30).IsRequired();
                b.Property(x => x.EmailId).HasColumnName("email_id");
                b.Property(x => x.FileName).HasColumnName("name").HasMaxLength(256).IsRequired();
                b.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(128).IsRequired();
                b.Property(x => x.Content).HasColumnName("content").IsRequired();
                b.Property(x => x.CreateDate).HasColumnName("create_time").HasColumnType("text").IsRequired();

                b.HasIndex(x => x.JobRunId)
                    .HasDatabaseName("idx_qrtz_job_run_attachment_job_run_id");
            });
    }
}