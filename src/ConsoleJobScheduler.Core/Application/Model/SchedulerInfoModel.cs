namespace ConsoleJobScheduler.Core.Application.Model;

public sealed class SchedulerInfoModel(SchedulerMetadataModel metadata, IList<SchedulerStateRecordModel> nodes)
{
    public SchedulerMetadataModel Metadata { get; } = metadata ?? throw new ArgumentNullException(nameof(metadata));

    public IList<SchedulerStateRecordModel> Nodes { get; } = nodes ?? throw new ArgumentNullException(nameof(nodes));
}