using Microsoft.AspNetCore.Http.HttpResults;

namespace ConsoleJobScheduler.Core.Domain.Runner.Model
{
    public sealed class JobRunLog
    {
        public long Id { get; set; }

        public string JobRunId { get; private set; }

        public string Content { get; private set; }

        public bool IsError { get; private set; }

        public DateTime CreateDate { get; private set; }

        public JobRunLog(string jobRunId, string content, bool isError, DateTime createDate)
        {
            JobRunId = jobRunId;
            IsError = isError;
            Content = content;
            CreateDate = createDate;
        }

        public static JobRunLog Created(string jobRunId, string content, bool isError)
        {
            return new JobRunLog(jobRunId, GetContent(content, isError), isError, DateTime.UtcNow);
        }

        private static string GetContent(string content, bool isError)
        {
            if (isError)
            {
                content ??= string.Empty;
                content = string.Join('\n', content.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => $"##[error] {x}"));
            }

            return content;
        }
    }
}