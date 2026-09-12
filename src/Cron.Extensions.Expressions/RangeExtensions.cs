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
    /// Sets the month component of the cron expression to "[start]-[end]", given by name.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="start">The first month name in the range (e.g. <c>"JAN"</c>).</param>
    /// <param name="end">The last month name in the range (e.g. <c>"JUN"</c>).</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the month component set to the provided range.</returns>
    /// <remarks>
    /// Accepts the three-letter names <c>JAN</c> through <c>DEC</c>, case-insensitively; an
    /// already-numeric string works too. Both values are normalized to their numeric equivalent
    /// together (not independently - see <see cref="ToNumbers"/>), then this delegates to
    /// <see cref="RangeOfMonths(CronExpression, int, int)"/>, so the assignment and validation
    /// live in exactly one place. See <see cref="CronExpression.Month"/> for the full translation rules.
    /// </remarks>
    public static CronExpression RangeOfMonths(this CronExpression expression, string start, string end)
    {
        var (s, e) = ToNumbers(start, end, Units.Month);
        return expression.RangeOfMonths(s, e);
    }

    /// <summary>
    /// Sets the month component of the cron expression to "[start]-[end]", with a numeric start and a named end.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="start">The first month value in the range.</param>
    /// <param name="end">The last month name in the range (e.g. <c>"JUN"</c>).</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the month component set to the provided range.</returns>
    /// <remarks>A mixed-form convenience over the two single-type overloads; see <see cref="CronExpression.Month"/> for the full translation rules.</remarks>
    public static CronExpression RangeOfMonths(this CronExpression expression, int start, string end)
    {
        var (s, e) = ToNumbers(start.ToString(), end, Units.Month);
        return expression.RangeOfMonths(s, e);
    }

    /// <summary>
    /// Sets the month component of the cron expression to "[start]-[end]", with a named start and a numeric end.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="start">The first month name in the range (e.g. <c>"JAN"</c>).</param>
    /// <param name="end">The last month value in the range.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the month component set to the provided range.</returns>
    /// <remarks>A mixed-form convenience over the two single-type overloads; see <see cref="CronExpression.Month"/> for the full translation rules.</remarks>
    public static CronExpression RangeOfMonths(this CronExpression expression, string start, int end)
    {
        var (s, e) = ToNumbers(start, end.ToString(), Units.Month);
        return expression.RangeOfMonths(s, e);
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

    /// <summary>
    /// Sets the day of the week component of the cron expression to "[start]-[end]", given by name.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="start">The first day-of-week name in the range (e.g. <c>"MON"</c>).</param>
    /// <param name="end">The last day-of-week name in the range (e.g. <c>"FRI"</c>, or <c>"SUN"</c> to close out a week).</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the day-of-week component set to the provided range.</returns>
    /// <remarks>
    /// Accepts the three-letter names <c>SUN</c> through <c>SAT</c>, case-insensitively; an
    /// already-numeric string works too. <c>SUN</c> as <paramref name="end"/> becomes <c>7</c>
    /// rather than <c>0</c> (e.g. <c>RangeOfWeek("MON", "SUN")</c> produces <c>"1-7"</c>, the
    /// whole week), since a range ending in <c>0</c> is always rejected as reversed. Both values
    /// are normalized together, not independently (see <see cref="ToNumbers"/>), which is exactly
    /// what makes that position-sensitive translation possible, then this delegates to
    /// <see cref="RangeOfWeek(CronExpression, int, int)"/>, so the assignment and validation live
    /// in exactly one place. See <see cref="CronExpression.DayOfWeek"/> for the full translation rules.
    /// </remarks>
    public static CronExpression RangeOfWeek(this CronExpression expression, string start, string end)
    {
        var (s, e) = ToNumbers(start, end, Units.DayOfWeek);
        return expression.RangeOfWeek(s, e);
    }

    /// <summary>
    /// Sets the day of the week component of the cron expression to "[start]-[end]", with a numeric start and a named end.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="start">The first day-of-week value in the range.</param>
    /// <param name="end">The last day-of-week name in the range (e.g. <c>"FRI"</c>, or <c>"SUN"</c> to close out a week).</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the day-of-week component set to the provided range.</returns>
    /// <remarks>A mixed-form convenience over the two single-type overloads; see <see cref="CronExpression.DayOfWeek"/> for the full translation rules.</remarks>
    public static CronExpression RangeOfWeek(this CronExpression expression, int start, string end)
    {
        var (s, e) = ToNumbers(start.ToString(), end, Units.DayOfWeek);
        return expression.RangeOfWeek(s, e);
    }

    /// <summary>
    /// Sets the day of the week component of the cron expression to "[start]-[end]", with a named start and a numeric end.
    /// </summary>
    /// <param name="expression">The cron expression being extended.</param>
    /// <param name="start">The first day-of-week name in the range (e.g. <c>"MON"</c>).</param>
    /// <param name="end">The last day-of-week value in the range.</param>
    /// <returns>The same <see cref="CronExpression"/> instance with the day-of-week component set to the provided range.</returns>
    /// <remarks>A mixed-form convenience over the two single-type overloads; see <see cref="CronExpression.DayOfWeek"/> for the full translation rules.</remarks>
    public static CronExpression RangeOfWeek(this CronExpression expression, string start, int end)
    {
        var (s, e) = ToNumbers(start, end.ToString(), Units.DayOfWeek);
        return expression.RangeOfWeek(s, e);
    }

    /// <summary>
    /// Translates a "start-end" pair to their numeric equivalents together, as a single range,
    /// rather than translating each side independently.
    /// </summary>
    /// <remarks>
    /// This distinction matters for <see cref="Units.DayOfWeek"/>: <c>SUN</c> means <c>7</c> only
    /// when it is the end of a range, so translating <paramref name="end"/> on its own - without
    /// the context that it is, in fact, the end of a range - would lose that rule and always
    /// produce <c>0</c> instead. <see cref="FieldNames.TranslateRange"/> is what preserves that
    /// position; this method only parses its result, it doesn't re-derive it - the "-" separator
    /// convention is <see cref="FieldNames"/>'s to know, not this class's.
    /// </remarks>
    private static (int Start, int End) ToNumbers(string start, string end, Units unit)
    {
        var (s, e) = FieldNames.TranslateRange(start, end, unit);
        return (int.Parse(s), int.Parse(e));
    }
}
