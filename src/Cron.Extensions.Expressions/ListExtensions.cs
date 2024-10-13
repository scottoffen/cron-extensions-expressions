namespace Cron.Extensions.Expressions;

public static class ListExtensions
{
    /// <summary>
    /// Sets the minute component of the cron expression to a list of minutes.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="minutes"></param>
    /// <remarks>Valid values are 0 to 59.</remarks>
    public static CronExpression OnMinutes(this CronExpression expression, params int[] minutes)
    {
        expression.Minute = string.Join(",", minutes.OrderBy(x => x).Distinct());
        return expression;
    }

    /// <summary>
    /// Sets the hour component of the cron expression to a list of hours.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="hours"></param>
    /// <remarks>Valid values are 0 to 23.</remarks>
    public static CronExpression OnHours(this CronExpression expression, params int[] hours)
    {
        expression.Hour = string.Join(",", hours.OrderBy(x => x).Distinct());
        return expression;
    }

    /// <summary>
    /// Sets the day component of the cron expression to a list of days.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="days"></param>
    /// <remarks>Valid values are 1 to 31</remarks>
    public static CronExpression OnDays(this CronExpression expression, params int[] days)
    {
        expression.Day = string.Join(",", days.OrderBy(x => x).Distinct());
        return expression;
    }

    /// <summary>
    /// Sets the month component of the cron expression to a list of months.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="months"></param>
    /// <remarks>Valid values are 1 to 12.</remarks>
    public static CronExpression OnMonths(this CronExpression expression, params int[] months)
    {
        expression.Month = string.Join(",", months.OrderBy(x => x).Distinct());
        return expression;
    }

    /// <summary>
    /// Sets the day of the week component of the cron expression to a list of days of the week.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="daysOfWeek"></param>
    /// <remarks>Valid values are 0 to 6; Sunday is 0 and Saturday is 6.</remarks>
    public static CronExpression OnDaysOfWeek(this CronExpression expression, params int[] daysOfWeek)
    {
        expression.DayOfWeek = string.Join(",", daysOfWeek.OrderBy(x => x).Distinct());
        return expression;
    }
}
