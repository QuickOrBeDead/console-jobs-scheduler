using System.Data;
using ConsoleJobScheduler.Core.Infra.Migration;
using ConsoleJobScheduler.Core.Infra.Migration.Extensions;
using FluentMigrator;
using FluentMigrator.Model;
using MigrationBase = ConsoleJobScheduler.Core.Infra.Migration.MigrationBase;

namespace ConsoleJobScheduler.Core.Infrastructure.Scheduler.Migrations;

[Tags("Scheduler")]
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

        IfDatabase("PostgreSQL").Create.ForeignKey(GetNameWithTablePrefix("blob_triggers_sched_name_trigger_name_trigger_group_fkey"))
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

        IfDatabase("PostgreSQL").Create.ForeignKey(GetNameWithTablePrefix("triggers_sched_name_job_name_job_group_fkey"))
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

        IfDatabase("PostgreSQL").Create.ForeignKey(GetNameWithTablePrefix("cron_triggers_sched_name_trigger_name_trigger_group_fkey"))
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

        // ------------------------------ locks ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("locks"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("lock_name").AsString().NotNullable().PrimaryKey();

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

        // ------------------------------ simple_triggers ----------------------------------------------
        Create.Table(GetNameWithTablePrefix("simple_triggers"))
            .WithColumn("sched_name").AsString().NotNullable().PrimaryKey()
            .WithColumn("trigger_name").AsString().Nullable().PrimaryKey()
            .WithColumn("trigger_group").AsString().NotNullable().PrimaryKey()
            .WithColumn("repeat_count").AsInt64().NotNullable()
            .WithColumn("repeat_interval").AsInt64().NotNullable()
            .WithColumn("times_triggered").AsInt64().NotNullable();

        IfDatabase("PostgreSQL").Create.ForeignKey(GetNameWithTablePrefix("simple_triggers_sched_name_trigger_name_trigger_group_fkey"))
            .FromTable(GetNameWithTablePrefix("simple_triggers")).ForeignColumns("sched_name", "trigger_name", "trigger_group")
            .ToTable(GetNameWithTablePrefix("triggers")).PrimaryColumns("sched_name", "trigger_name", "trigger_group")
            .OnUpdate(Rule.None)
            .OnDelete(Rule.Cascade);

        // ------------------------------ simprop_triggers ----------------------------------------------
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

        IfDatabase("PostgreSQL").Create.ForeignKey(GetNameWithTablePrefix("simprop_triggers_sched_name_trigger_name_trigger_grou_fkey"))
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