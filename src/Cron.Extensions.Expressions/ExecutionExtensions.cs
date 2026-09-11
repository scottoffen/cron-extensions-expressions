namespace Cron.Extensions.Expressions;

/// <summary>
/// Provides extension methods for executing and evaluating cron expressions.
/// </summary>
public static class ExecutionExtensions
{
    private static readonly string _wildcard = "*";

    /// <summary>
    /// Get the next execution time of the cron expression from the start date. If no start date is provided, the current date is used.
    /// </summary>
    /// <remarks>
    /// Time arithmetic uses <see cref="DateTime"/> addition, which does not apply daylight saving time (DST) rules.
    /// When <paramref name="start"/> has <see cref="DateTime.Kind"/> <see cref="DateTimeKind.Local"/> and the next run
    /// falls in a skipped hour (e.g. 2:00 AM on spring-forward day), the returned time may be invalid in the local zone.
    /// When it falls in a repeated hour (e.g. 1:30 AM on fall-back day), the result may be ambiguous.
    /// For predictable behavior across DST boundaries, use a <paramref name="start"/> with <see cref="DateTimeKind.Utc"/>.
    /// </remarks>
    /// <param name="expression">The cron expression to evaluate.</param>
    /// <param name="start">The date and time to evaluate from. When <c>null</c>, the current local time is used.</param>
    /// <returns>The next scheduled execution time for the specified cron expression.</returns>
    public static DateTime GetNextExecution(this CronExpression expression, DateTime? start = null)
    {
        var now = DateTime.Now;
        var next = start ?? now;
        start ??= now;

        while (true)
        {
            if (!CanExecute(next.Minute, expression.Minute))
            {
                next = next
                    .AddMinutes(1);
                continue;
            }

            if (!CanExecute(next.Hour, expression.Hour))
            {
                next = next
                    .AddMinutes(-next.Minute)
                    .AddHours(1);
                continue;
            }

            if (!CanExecute(next.Day, expression.Day))
            {
                next = next
                    .AddMinutes(-next.Minute)
                    .AddHours(-next.Hour)
                    .AddDays(1);
                continue;
            }

            if (!CanExecute(next.Month, expression.Month))
            {
                next = next
                    .AddMinutes(-next.Minute)
                    .AddHours(-next.Hour)
                    .AddDays(-next.Day + 1)
                    .AddMonths(1);
                continue;
            }

            if (!CanExecute((int)next.DayOfWeek, expression.DayOfWeek))
            {
                next = next
                    .AddMinutes(-next.Minute)
                    .AddHours(-next.Hour)
                    .AddDays(1);
                continue;
            }

            if (next == start)
            {
                next = next.AddMinutes(1);
                continue;
            }

            return new DateTime(next.Year, next.Month, next.Day, next.Hour, next.Minute, 0, next.Kind);
        }
    }

    /// <summary>
    /// Check if the cron expression will run on the provided date and time.
    /// </summary>
    /// <param name="expression">The cron expression to evaluate.</param>
    /// <param name="date">The date and time to test against the cron expression.</param>
    /// <returns><c>true</c> when the cron expression matches the provided date and time; otherwise, <c>false</c>.</returns>
    public static bool WillRunOn(this CronExpression expression, DateTime date)
    {
        return CanExecute(date.Minute, expression.Minute)
               && CanExecute(date.Hour, expression.Hour)
               && CanExecute(date.Day, expression.Day)
               && CanExecute(date.Month, expression.Month)
               && CanExecute((int)date.DayOfWeek, expression.DayOfWeek);
    }

    private static bool CanExecute(int value, string expression)
    {
        if (expression == _wildcard) return true;

        if (expression.Contains(','))
        {
            var values = expression.Split(',');
            return values.Any(v => CanExecute(value, v));
        }

        if (expression.Contains('-'))
        {
            var values = expression.Split('-');
            return value >= int.Parse(values[0]) && value <= int.Parse(values[1]);
        }

        if (expression.Contains('/'))
        {
            var values = expression.Split('/');
            var start = (values[0] == _wildcard) ? 0 : int.Parse(values[0]);
            var step = int.Parse(values[1]);

            return (value - start) % step == 0;
        }

        return value == int.Parse(expression);
    }
}
