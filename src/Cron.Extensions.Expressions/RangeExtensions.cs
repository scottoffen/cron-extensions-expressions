namespace Cron.Extensions.Expressions;

/// <summary>
/// Provides extension methods for setting range values in cron expressions.
/// </summary>
public static class RangeExtensions
{
    /// <summary>
    /// Sets the minute component of the cron expression to "[start]-[end]".
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="start">The first minute value in the range.</param>
    /// <param name="end">The last minute value in the range.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the minute component set to the provided range.</returns>
    /// <remarks>Valid values for start and end are 0 to 59</remarks>
    public static CronExpression RangeOfMinutes(this CronExpression expression, int start, int end)
    {
        expression.Minute = $"{start}-{end}";
        return expression;
    }

    /// <summary>
    /// Sets the hour component of the cron expression to "[start]-[end]".
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="start">The first hour value in the range.</param>
    /// <param name="end">The last hour value in the range.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the hour component set to the provided range.</returns>
    /// <remarks>Valid values for start and end are 0 to 23</remarks>
    public static CronExpression RangeOfHours(this CronExpression expression, int start, int end)
    {
        expression.Hour = $"{start}-{end}";
        return expression;
    }

    /// <summary>
    /// Sets the day component of the cron expression to "[start]-[end]".
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="start">The first day-of-month value in the range.</param>
    /// <param name="end">The last day-of-month value in the range.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the day component set to the provided range.</returns>
    /// <remarks>Valid values for start and end are 1 to 31</remarks>
    public static CronExpression RangeOfDays(this CronExpression expression, int start, int end)
    {
        expression.Day = $"{start}-{end}";
        return expression;
    }

    /// <summary>
    /// Sets the month component of the cron expression to "[start]-[end]".
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="start">The first month value in the range.</param>
    /// <param name="end">The last month value in the range.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the month component set to the provided range.</returns>
    /// <remarks>Valid values for start and end are 1 to 12</remarks>
    public static CronExpression RangeOfMonths(this CronExpression expression, int start, int end)
    {
        expression.Month = $"{start}-{end}";
        return expression;
    }

    /// <summary>
    /// Sets the day of the week component of the cron expression to "[start]-[end]".
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="start">The first day-of-week value in the range.</param>
    /// <param name="end">The last day-of-week value in the range.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the day-of-week component set to the provided range.</returns>
    /// <remarks>Valid values for start and end are 0 to 6; Sunday is 0 and Saturday is 6.</remarks>
    public static CronExpression RangeOfWeek(this CronExpression expression, int start, int end)
    {
        expression.DayOfWeek = $"{start}-{end}";
        return expression;
    }
}
