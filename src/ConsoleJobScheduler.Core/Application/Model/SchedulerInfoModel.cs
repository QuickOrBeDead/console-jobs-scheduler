namespace ConsoleJobScheduler.Core.Application.Model;

public sealed class SchedulerInfoModel
{
    public SchedulerMetadataModel Metadata { get; }

    public IList<SchedulerStateRecordModel> Nodes { get; }

    public SchedulerInfoModel(SchedulerMetadataModel metadata, IList<SchedulerStateRecordModel> nodes)
    {
        Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }
}