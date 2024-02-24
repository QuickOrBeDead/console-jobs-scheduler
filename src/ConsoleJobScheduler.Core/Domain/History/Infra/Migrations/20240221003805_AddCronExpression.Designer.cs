﻿// <auto-generated />
using System;
using ConsoleJobScheduler.Core.Domain.History.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ConsoleJobScheduler.Core.Domain.History.Infra.Migrations
{
    [DbContext(typeof(HistoryDbContext))]
    [Migration("20240221003805_AddCronExpression")]
    partial class AddCronExpression
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ConsoleJobScheduler.Core.Domain.History.Model.JobExecutionHistory", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text")
                        .HasColumnName("id");

                    b.Property<bool>("Completed")
                        .HasColumnType("boolean")
                        .HasColumnName("completed");

                    b.Property<string>("CronExpressionString")
                        .HasColumnType("text")
                        .HasColumnName("cron_expression");

                    b.Property<string>("ErrorDetails")
                        .HasColumnType("text")
                        .HasColumnName("error_details");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text")
                        .HasColumnName("error_message");

                    b.Property<DateTime>("FiredTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("fired_time");

                    b.Property<bool>("HasError")
                        .HasColumnType("boolean")
                        .HasColumnName("has_error");

                    b.Property<string>("InstanceName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("instance_name");

                    b.Property<string>("JobGroup")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("job_group");

                    b.Property<string>("JobName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("job_name");

                    b.Property<DateTime>("LastSignalTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_signal_time");

                    b.Property<DateTime?>("NextFireTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("next_fire_time");

                    b.Property<string>("PackageName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("package_name");

                    b.Property<TimeSpan?>("RunTime")
                        .HasColumnType("interval")
                        .HasColumnName("run_time");

                    b.Property<DateTime?>("ScheduledTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("sched_time");

                    b.Property<string>("SchedulerName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("sched_name");

                    b.Property<string>("TriggerGroup")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("trigger_group");

                    b.Property<string>("TriggerName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("trigger_name");

                    b.Property<bool>("Vetoed")
                        .HasColumnType("boolean")
                        .HasColumnName("vetoed");

                    b.HasKey("Id");

                    b.ToTable("qrtz_job_history", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
