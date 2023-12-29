namespace ConsoleJobScheduler.Core.Api.Models;

public sealed class SchedulerInfoModel
{
    public SchedulerMetadataModel Metadata { get; }

    public IList<SchedulerStateRecordModel> Nodes { get; }

    public SchedulerJobExecutionStatisticsModel Statistics { get; }

    public SchedulerInfoModel(SchedulerMetadataModel metadata, IList<SchedulerStateRecordModel> nodes, SchedulerJobExecutionStatisticsModel statistics)
    {
        Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        Statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }
}