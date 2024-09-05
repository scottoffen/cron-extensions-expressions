namespace Cron.Extensions.Expressions;

public static class RangeExtensions
{
    /// <summary>
    /// Sets the minute component of the cron expression to "[start]-[end]".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <remarks>Valid values for start and end are 0 to 59</remarks>
    public static CronExpression RangeOfMinutes(this CronExpression expression, int start, int end)
    {
        expression.Minute = $"{start}-{end}";
        return expression;
    }

    /// <summary>
    /// Sets the hour component of the cron expression to "[start]-[end]".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <remarks>Valid values for start and end are 0 to 23</remarks>
    public static CronExpression RangeOfHours(this CronExpression expression, int start, int end)
    {
        expression.Hour = $"{start}-{end}";
        return expression;
    }

    /// <summary>
    /// Sets the day component of the cron expression to "[start]-[end]".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <remarks>Valid values for start and end are 1 to 31</remarks>
    public static CronExpression RangeOfDays(this CronExpression expression, int start, int end)
    {
        expression.Day = $"{start}-{end}";
        return expression;
    }

    /// <summary>
    /// Sets the month component of the cron expression to "[start]-[end]".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <remarks>Valid values for start and end are 1 to 12</remarks>
    public static CronExpression RangeOfMonths(this CronExpression expression, int start, int end)
    {
        expression.Month = $"{start}-{end}";
        return expression;
    }

    /// <summary>
    /// Sets the day of the week component of the cron expression to "[start]-[end]".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <remarks>Valid values for start and end are 0 to 6; Sunday is 0 and Saturday is 6.</remarks>
    public static CronExpression RangeOfWeek(this CronExpression expression, int start, int end)
    {
        expression.DayOfWeek = $"{start}-{end}";
        return expression;
    }
}
