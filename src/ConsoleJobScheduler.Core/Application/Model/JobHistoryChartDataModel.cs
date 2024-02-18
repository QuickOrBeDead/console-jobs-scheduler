using System.Diagnostics.CodeAnalysis;

namespace ConsoleJobScheduler.Core.Application.Model
{
    public sealed class JobHistoryChartDataModel
    {
        [NotNull]
        public DateTime X { get; set; }

        [NotNull]
        public int Y { get; set; }
    }
}