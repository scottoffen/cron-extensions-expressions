namespace Cron.Extensions.Expressions;

/// <summary>
/// Provides extension methods for setting list values in cron expressions.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Sets the minute component of the cron expression to a list of minutes.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="minutes">The minute values to include in the cron schedule.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the minute component set to the provided list.</returns>
    /// <remarks>Valid values are 0 to 59.</remarks>
    public static CronExpression OnMinutes(this CronExpression expression, params int[] minutes)
    {
        expression.Minute = string.Join(",", minutes.OrderBy(x => x).Distinct());
        return expression;
    }

    /// <summary>
    /// Sets the hour component of the cron expression to a list of hours.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="hours">The hour values to include in the cron schedule.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the hour component set to the provided list.</returns>
    /// <remarks>Valid values are 0 to 23.</remarks>
    public static CronExpression OnHours(this CronExpression expression, params int[] hours)
    {
        expression.Hour = string.Join(",", hours.OrderBy(x => x).Distinct());
        return expression;
    }

    /// <summary>
    /// Sets the day component of the cron expression to a list of days.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="days">The day-of-month values to include in the cron schedule.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the day component set to the provided list.</returns>
    /// <remarks>Valid values are 1 to 31</remarks>
    public static CronExpression OnDays(this CronExpression expression, params int[] days)
    {
        expression.Day = string.Join(",", days.OrderBy(x => x).Distinct());
        return expression;
    }

    /// <summary>
    /// Sets the month component of the cron expression to a list of months.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="months">The month values to include in the cron schedule.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the month component set to the provided list.</returns>
    /// <remarks>Valid values are 1 to 12.</remarks>
    public static CronExpression OnMonths(this CronExpression expression, params int[] months)
    {
        expression.Month = string.Join(",", months.OrderBy(x => x).Distinct());
        return expression;
    }

    /// <summary>
    /// Sets the month component of the cron expression to a list of months, given by name.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="months">The month names to include in the cron schedule (e.g. <c>"JAN"</c>).</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the month component set to the provided list.</returns>
    /// <remarks>
    /// Accepts the three-letter names <c>JAN</c> through <c>DEC</c>, case-insensitively, and an
    /// already-numeric string (e.g. <c>"6"</c>) works too, since each value is resolved the same
    /// way <see cref="CronExpression.Month"/> resolves it. Each name is normalized to its numeric
    /// value and this then delegates to <see cref="OnMonths(CronExpression, int[])"/>, so the
    /// sorting, deduplication, and validation live in exactly one place. Calling this with zero
    /// values is not supported - it resolves to this overload only when
    /// <see cref="OnMonths(CronExpression, int[])"/> also could, at which point the call is
    /// ambiguous and neither is used in practice.
    /// </remarks>
    public static CronExpression OnMonths(this CronExpression expression, params string[] months) =>
        expression.OnMonths(ToNumbers(months, Units.Month));

    /// <summary>
    /// Sets the day of the week component of the cron expression to a list of days of the week.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="daysOfWeek">The day-of-week values to include in the cron schedule.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the day-of-week component set to the provided list.</returns>
    /// <remarks>Valid values are 0 to 6; Sunday is 0 and Saturday is 6.</remarks>
    public static CronExpression OnDaysOfWeek(this CronExpression expression, params int[] daysOfWeek)
    {
        expression.DayOfWeek = string.Join(",", daysOfWeek.OrderBy(x => x).Distinct());
        return expression;
    }

    /// <summary>
    /// Sets the day of the week component of the cron expression to a list of days of the week, given by name.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="daysOfWeek">The day-of-week names to include in the cron schedule (e.g. <c>"MON"</c>).</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the day-of-week component set to the provided list.</returns>
    /// <remarks>
    /// Accepts the three-letter names <c>SUN</c> through <c>SAT</c>, case-insensitively, and an
    /// already-numeric string (e.g. <c>"6"</c>) works too, since each value is resolved the same
    /// way <see cref="CronExpression.DayOfWeek"/> resolves it. Every name here is a standalone
    /// list value, not the end of a range, so <c>SUN</c> always means <c>0</c> in this overload -
    /// the end-of-range translation of <c>SUN</c> to <c>7</c> only applies within a single
    /// <c>RangeOfWeek</c> value, never across separate list entries. Each name is normalized to
    /// its numeric value and this then delegates to
    /// <see cref="OnDaysOfWeek(CronExpression, int[])"/>, so the sorting, deduplication, and
    /// validation live in exactly one place.
    /// </remarks>
    public static CronExpression OnDaysOfWeek(this CronExpression expression, params string[] daysOfWeek) =>
        expression.OnDaysOfWeek(ToNumbers(daysOfWeek, Units.DayOfWeek));

    private static int[] ToNumbers(string[] values, Units unit) =>
        values.Select(v => int.Parse(FieldNames.Translate(v, unit))).ToArray();
}
