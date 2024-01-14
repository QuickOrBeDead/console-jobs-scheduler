using System.Data;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Migrations.Core;
using ConsoleJobScheduler.Core.Infrastructure.Scheduler.Migrations.Core.Extensions;
using FluentMigrator;
using FluentMigrator.Model;
using MigrationBase = ConsoleJobScheduler.Core.Infrastructure.Scheduler.Migrations.Core.MigrationBase;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Migrations;

[Migration(1, TransactionBehavior.Default)]
public class InitialSetup : MigrationBase
{
    public InitialSetup(IMigrationContext migrationContext) : base(migrationContext)
    {
    }

    public override void Up()
    {
        // ------------------------------ blob_triggers ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("blob_triggers"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_group").AsString().NotNullable().PrimaryKey()
            .WithColumn("blob_data").AsBytea().Nullable();

        // ------------------------------ triggers ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("triggers"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_group").AsString().NotNullable().PrimaryKey()
            .WithColumn("job_name").AsString().NotNullable()
            .WithColumn("job_group").AsString().NotNullable()
            .WithColumn("description").AsString().Nullable()
            .WithColumn("next_fire_time").AsInt64().Nullable()
            .WithColumn("prev_fire_time").AsInt64().Nullable()
            .WithColumn("priority").AsInt32().Nullable()
            .WithColumn("trigger_state").AsString().NotNullable()
            .WithColumn("trigger_type").AsString().NotNullable()
            .WithColumn("start_time").AsInt64().NotNullable()
            .WithColumn("end_time").AsInt64().Nullable()
            .WithColumn("calendar_name").AsString().Nullable()
            .WithColumn("misfire_instr").AsInt16().Nullable()
            .WithColumn("job_data").AsBytea().Nullable();

        Create.Index(GetIndexNameWithTablePrefix("t_next_fire_time"))
            .OnTable(GetNameWithTablePrefix("triggers"))
            .OnColumn("next_fire_time").Ascending()
            .WithOptions().NonClustered();

        Create.Index(GetIndexNameWithTablePrefix("t_nft_st"))
            .OnTable(GetNameWithTablePrefix("triggers"))
            .OnColumn("next_fire_time").Ascending()
            .OnColumn("trigger_state").Ascending()
            .WithOptions().NonClustered();

        Create.Index(GetIndexNameWithTablePrefix("t_state"))
            .OnTable(GetNameWithTablePrefix("triggers"))
            .OnColumn("trigger_state").Ascending()
            .WithOptions().NonClustered();

        Create.ForeignKey(GetNameWithTablePrefix("blob_triggers_sched_name_trigger_name_trigger_group_fkey"))
            .FromTable(GetNameWithTablePrefix("blob_triggers")).ForeignColumns("sched_name", "trigger_name", "trigger_group")
            .ToTable(GetNameWithTablePrefix("triggers")).PrimaryColumns("sched_name", "trigger_name", "trigger_group")
            .OnUpdate(Rule.None)
            .OnDelete(Rule.Cascade);

        // ------------------------------ job_details ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("job_details"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("job_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("job_group").AsString().NotNullable().PrimaryKey()
            .WithColumn("description").AsString().Nullable()
            .WithColumn("job_class_name").AsString().NotNullable()
            .WithColumn("is_durable").AsBoolean().NotNullable()
            .WithColumn("is_nonconcurrent").AsBoolean().NotNullable()
            .WithColumn("is_update_data").AsBoolean().NotNullable()
            .WithColumn("requests_recovery").AsBoolean().NotNullable()
            .WithColumn("job_data").AsBytea().Nullable();

        Create.Index(GetIndexNameWithTablePrefix("j_req_recovery")).OnTable(GetNameWithTablePrefix("job_details"))
            .OnColumn("requests_recovery").Ascending()
            .WithOptions().NonClustered();

        Create.ForeignKey(GetNameWithTablePrefix("triggers_sched_name_job_name_job_group_fkey"))
            .FromTable(GetNameWithTablePrefix("triggers")).ForeignColumns("sched_name", "job_name", "job_group")
            .ToTable(GetNameWithTablePrefix("job_details")).PrimaryColumns("sched_name", "job_name", "job_group")
            .OnUpdate(Rule.None)
            .OnDelete(Rule.None);

        // ------------------------------ calendars ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("calendars"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("calendar_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("calendar").AsBytea().NotNullable();

        // ------------------------------ cron_triggers ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("cron_triggers"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_group").AsString().NotNullable().PrimaryKey()
            .WithColumn("cron_expression").AsString().NotNullable()
            .WithColumn("time_zone_id").AsString().Nullable();

        Create.ForeignKey(GetNameWithTablePrefix("cron_triggers_sched_name_trigger_name_trigger_group_fkey"))
            .FromTable(GetNameWithTablePrefix("cron_triggers")).ForeignColumns("sched_name", "trigger_name", "trigger_group")
            .ToTable(GetNameWithTablePrefix("triggers")).PrimaryColumns("sched_name", "trigger_name", "trigger_group")
            .OnUpdate(Rule.None)
            .OnDelete(Rule.Cascade);

        // ------------------------------ fired_triggers ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("fired_triggers"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("entry_id").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString().NotNullable()
            .WithColumn("trigger_group").AsString().NotNullable()
            .WithColumn("instance_name").AsString().NotNullable()
            .WithColumn("fired_time").AsInt64().NotNullable()
            .WithColumn("sched_time").AsInt64().NotNullable()
            .WithColumn("priority").AsInt32().NotNullable()
            .WithColumn("state").AsString().NotNullable()
            .WithColumn("job_name").AsString().Nullable()
            .WithColumn("job_group").AsString().Nullable()
            .WithColumn("is_nonconcurrent").AsBoolean().NotNullable()
            .WithColumn("requests_recovery").AsBoolean().Nullable();

        Create.Index(GetIndexNameWithTablePrefix("ft_job_group"))
            .OnTable(GetNameWithTablePrefix("fired_triggers"))
            .OnColumn("job_group").Ascending()
            .WithOptions().NonClustered();

        Create.Index(GetIndexNameWithTablePrefix("ft_job_name"))
            .OnTable(GetNameWithTablePrefix("fired_triggers"))
            .OnColumn("job_name").Ascending()
            .WithOptions().NonClustered();

        Create.Index(GetIndexNameWithTablePrefix("ft_job_req_recovery"))
            .OnTable(GetNameWithTablePrefix("fired_triggers"))
            .OnColumn("requests_recovery").Ascending()
            .WithOptions().NonClustered();

        Create.Index(GetIndexNameWithTablePrefix("ft_trig_group"))
            .OnTable(GetNameWithTablePrefix("fired_triggers"))
            .OnColumn("trigger_group").Ascending()
            .WithOptions().NonClustered();

        Create.Index(GetIndexNameWithTablePrefix("ft_trig_inst_name"))
            .OnTable(GetNameWithTablePrefix("fired_triggers"))
            .OnColumn("instance_name").Ascending()
            .WithOptions().NonClustered();

        Create.Index(GetIndexNameWithTablePrefix("ft_trig_name"))
            .OnTable(GetNameWithTablePrefix("fired_triggers"))
            .OnColumn("trigger_name").Ascending()
            .WithOptions().NonClustered();

        Create.Index(GetIndexNameWithTablePrefix("ft_trig_nm_gp"))
            .OnTable(GetNameWithTablePrefix("fired_triggers"))
            .OnColumn("sched_name").Ascending()
            .OnColumn("trigger_name").Ascending()
            .OnColumn("trigger_group").Ascending()
            .WithOptions().NonClustered();

        // ------------------------------ job_history ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("job_history"))
            .WithColumn("id").AsString(30).NotNullable().PrimaryKey()
            .WithColumn("sched_name").AsString().NotNullable()
            .WithColumn("instance_name").AsString().NotNullable()
            .WithColumn("job_name").AsString().NotNullable()
            .WithColumn("job_group").AsString().NotNullable()
            .WithColumn("package_name").AsString().Nullable()
            .WithColumn("trigger_name").AsString().NotNullable()
            .WithColumn("trigger_group").AsString().NotNullable()
            .WithColumn("fired_time").AsInt64().NotNullable()
            .WithColumn("sched_time").AsInt64().NotNullable()
            .WithColumn("last_signal_time").AsInt64().NotNullable()
            .WithColumn("run_time").AsInt64().Nullable()
            .WithColumn("has_error").AsBoolean().NotNullable()
            .WithColumn("error_message").AsString().Nullable()
            .WithColumn("error_details").AsString().Nullable()
            .WithColumn("vetoed").AsBoolean().NotNullable()
            .WithColumn("completed").AsBoolean().NotNullable();

        // ------------------------------ job_run_attachment ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("job_run_attachment"))
            .WithColumn("id").AsInt64().NotNullable().Identity().PrimaryKey()
            .WithColumn("job_run_id").AsString(30).NotNullable()
            .WithColumn("email_id").AsGuid().Nullable()
            .WithColumn("name").AsString(256).NotNullable()
            .WithColumn("content_type").AsString(128).NotNullable()
            .WithColumn("content").AsBytea().NotNullable()
            .WithColumn("create_time").AsInt64().NotNullable();

        Create.Index(GetIndexNameWithTablePrefix("job_run_attachment_job_run_id"))
            .OnTable(GetNameWithTablePrefix("job_run_attachment"))
            .OnColumn("job_run_id").Ascending()
            .WithOptions().NonClustered();

        // ------------------------------ job_run_email ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("job_run_email"))
            .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
            .WithColumn("job_run_id").AsString(30).NotNullable()
            .WithColumn("subject").AsString(256).NotNullable()
            .WithColumn("body").AsString().NotNullable()
            .WithColumn("message_to").AsString().NotNullable()
            .WithColumn("message_cc").AsString().NotNullable()
            .WithColumn("message_bcc").AsString().NotNullable()
            .WithColumn("is_sent").AsBoolean().NotNullable()
            .WithColumn("create_time").AsInt64().NotNullable();

        Create.Index(GetIndexNameWithTablePrefix("job_run_email_job_run_id"))
            .OnTable(GetNameWithTablePrefix("job_run_email"))
            .OnColumn("job_run_id").Ascending()
            .WithOptions().NonClustered();

        // ------------------------------ job_run_log ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("job_run_log"))
            .WithColumn("id").AsInt64().NotNullable().Identity().PrimaryKey()
            .WithColumn("job_run_id").AsString(30).NotNullable()
            .WithColumn("content").AsString().Nullable()
            .WithColumn("is_error").AsBoolean().NotNullable()
            .WithColumn("create_time").AsInt64().NotNullable();

        Create.Index(GetIndexNameWithTablePrefix("job_run_log_job_run_id"))
            .OnTable(GetNameWithTablePrefix("job_run_log"))
            .OnColumn("job_run_id").Ascending()
            .WithOptions().NonClustered();

        // ------------------------------ locks ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("locks"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("lock_name").AsString().NotNullable().PrimaryKey();

        // ------------------------------ packages ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("packages"))
            .WithColumn("name").AsString(256).NotNullable().PrimaryKey()
            .WithColumn("content").AsBytea().NotNullable()
            .WithColumn("create_time").AsInt64().NotNullable()
            .WithColumn("file_name").AsString(1024).NotNullable()
            .WithColumn("arguments").AsString(1024).NotNullable()
            .WithColumn("author").AsString(50).NotNullable()
            .WithColumn("description").AsString(1024).NotNullable()
            .WithColumn("version").AsString(50).NotNullable();

        // ------------------------------ paused_trigger_grps ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("paused_trigger_grps"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_group").AsString().NotNullable().PrimaryKey();

        // ------------------------------ scheduler_state ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("scheduler_state"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("instance_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("last_checkin_time").AsInt64().NotNullable()
            .WithColumn("checkin_interval").AsInt64().NotNullable();

        // ------------------------------ settings ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("settings"))
            .WithColumn("CategoryId").AsInt32().NotNullable().PrimaryKey()
            .WithColumn("Name").AsString(255).NotNullable().PrimaryKey()
            .WithColumn("Value").AsString().Nullable();

        // ------------------------------ simple_triggers ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("simple_triggers"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString().Nullable().PrimaryKey()
            .WithColumn("trigger_group").AsString().NotNullable().PrimaryKey()
            .WithColumn("repeat_count").AsInt64().NotNullable()
            .WithColumn("repeat_interval").AsInt64().NotNullable()
            .WithColumn("times_triggered").AsInt64().NotNullable();

        Create.ForeignKey(GetNameWithTablePrefix("simple_triggers_sched_name_trigger_name_trigger_group_fkey"))
            .FromTable(GetNameWithTablePrefix("simple_triggers")).ForeignColumns("sched_name", "trigger_name", "trigger_group")
            .ToTable(GetNameWithTablePrefix("triggers")).PrimaryColumns("sched_name", "trigger_name", "trigger_group")
            .OnUpdate(Rule.None)
            .OnDelete(Rule.Cascade);

        // ------------------------------ job_history ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("simprop_triggers"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_group").AsString().NotNullable().PrimaryKey()
            .WithColumn("str_prop_1").AsString().Nullable()
            .WithColumn("str_prop_2").AsString().Nullable()
            .WithColumn("str_prop_3").AsString().Nullable()
            .WithColumn("int_prop_1").AsInt32().Nullable()
            .WithColumn("int_prop_2").AsInt32().Nullable()
            .WithColumn("long_prop_1").AsInt64().Nullable()
            .WithColumn("long_prop_2").AsInt64().Nullable()
            .WithColumn("dec_prop_1").AsCustom("numeric").Nullable()
            .WithColumn("dec_prop_2").AsCustom("numeric").Nullable()
            .WithColumn("bool_prop_1").AsBoolean().Nullable()
            .WithColumn("bool_prop_2").AsBoolean().Nullable()
            .WithColumn("time_zone_id").AsString().Nullable();

        Create.ForeignKey(GetNameWithTablePrefix("simprop_triggers_sched_name_trigger_name_trigger_grou_fkey"))
            .FromTable(GetNameWithTablePrefix("simprop_triggers")).ForeignColumns("sched_name", "trigger_name", "trigger_group")
            .ToTable(GetNameWithTablePrefix("triggers")).PrimaryColumns("sched_name", "trigger_name", "trigger_group")
            .OnUpdate(Rule.None)
            .OnDelete(Rule.Cascade);
    }

    public override void Down()
    {
        // empty
    }
}