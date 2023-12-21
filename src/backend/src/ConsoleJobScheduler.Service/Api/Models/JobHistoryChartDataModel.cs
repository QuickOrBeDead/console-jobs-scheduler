using System.Diagnostics.CodeAnalysis;

namespace ConsoleJobScheduler.Service.Api.Models
{
    public sealed class JobHistoryChartDataModel
    {
        [NotNull]
        public DateTime X { get; set; }

        [NotNull]
        public int Y { get; set; }
    }
}