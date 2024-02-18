namespace ConsoleJobScheduler.Core.Domain.Scheduler.Model
{
    public sealed class JobCronExpression
    {
        public string Expression { get; private set; }

        public string Description { get; private set; }
        
        public JobCronExpression(string expression, string description)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }
}