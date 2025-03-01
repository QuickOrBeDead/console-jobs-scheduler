namespace ConsoleJobScheduler.Core.Infra;

public static class DateTimeExtensions
{
    public static DateTime ToUtc(this DateTime value)
    {
        if (value.Kind == DateTimeKind.Unspecified)
        {
            value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        else if (value.Kind == DateTimeKind.Local)
        {
            value = value.ToUniversalTime();
        }
        return value;
    }
}
