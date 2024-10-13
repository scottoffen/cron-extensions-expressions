namespace Cron.Extensions.Expressions;

public static class IncrementExtensions
{
    /// <summary>
    /// Sets the minute component of the cron expression to "*".
    /// </summary>
    /// <param name="expression"></param>
    public static CronExpression EveryMinute(this CronExpression expression)
    {
        expression.Minute = "*";
        return expression;
    }

    /// <summary>
    /// Sets the minute component of the cron expression to "*/increment".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="increment"></param>
    /// <remarks>Valid values are 0 to 59</remarks>
    public static CronExpression EveryXMinutes(this CronExpression expression, int increment)
    {
        if (increment == 1) return expression.EveryMinute();
        expression.Minute = $"*/{increment}";
        return expression;
    }

    /// <summary>
    /// Sets the minute component of the cron expression to "start/minutes".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="start"></param>
    /// <param name="increment"></param>
    /// <remarks>Valid values for start and increment are 0 to 59</remarks>
    public static CronExpression EveryXMinutes(this CronExpression expression, int start, int increment)
    {
        if (start == 1 && increment == 1) return expression.EveryMinute();
        expression.Minute = $"{start}/{increment}";
        return expression;
    }

    /// <summary>
    /// Sets the hour component of the cron expression to "*".
    /// </summary>
    /// <param name="expression"></param>
    public static CronExpression EveryHour(this CronExpression expression)
    {
        expression.Hour = "*";
        return expression;
    }

    /// <summary>
    /// Sets the hour component of the cron expression to "*/increment".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="increment"></param>
    /// <remarks>Valid values are 0 to 23.</remarks>
    public static CronExpression EveryXHours(this CronExpression expression, int increment)
    {
        if (increment == 1) return expression.EveryHour();
        expression.Hour = $"*/{increment}";
        return expression;
    }

    /// <summary>
    /// Sets the hour component of the cron expression to "start/hours".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="start"></param>
    /// <param name="increment"></param>
    /// <remarks>Valid values for start and increment are 0 to 23.</remarks>
    public static CronExpression EveryXHours(this CronExpression expression, int start, int increment)
    {
        if (start == 1 && increment == 1) return expression.EveryHour();
        expression.Hour = $"{start}/{increment}";
        return expression;
    }

    /// <summary>
    /// Sets the day component of the cron expression to "*".
    /// </summary>
    /// <param name="expression"></param>
    public static CronExpression EveryDay(this CronExpression expression)
    {
        expression.Day = "*";
        return expression;
    }

    /// <summary>
    /// Sets the day component of the cron expression to "*/increment".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="increment"></param>
    /// <remarks>Valid values are 1 to 31.</remarks>
    public static CronExpression EveryXDays(this CronExpression expression, int increment)
    {
        if (increment == 1) return expression.EveryDay();
        expression.Day = $"*/{increment}";
        return expression;
    }

    /// <summary>
    /// Sets the day component of the cron expression to "start/days".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="start"></param>
    /// <param name="increment"></param>
    /// <remarks>Valid values for start and increment are 1 to 31.</remarks>
    public static CronExpression EveryXDays(this CronExpression expression, int start, int increment)
    {
        if (start == 1 && increment == 1) return expression.EveryDay();
        expression.Day = $"{start}/{increment}";
        return expression;
    }

    /// <summary>
    /// Sets the month component of the cron expression to "*".
    /// </summary>
    /// <param name="expression"></param>
    public static CronExpression EveryMonth(this CronExpression expression)
    {
        expression.Month = "*";
        return expression;
    }

    /// <summary>
    /// Sets the month component of the cron expression to "*/increment".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="increment"></param>
    /// <remarks>Valid values are 1 to 12.</remarks>
    public static CronExpression EveryXMonths(this CronExpression expression, int increment)
    {
        if (increment == 1) return expression.EveryMonth();
        expression.Month = $"*/{increment}";
        return expression;
    }

    /// <summary>
    /// Sets the month component of the cron expression to "start/months".
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="start"></param>
    /// <param name="increment"></param>
    /// <remarks>Valid values for start and increment are 1 to 12.</remarks>
    public static CronExpression EveryXMonths(this CronExpression expression, int start, int increment)
    {
        if (start == 1 && increment == 1) return expression.EveryMonth();
        expression.Month = $"{start}/{increment}";
        return expression;
    }
}

