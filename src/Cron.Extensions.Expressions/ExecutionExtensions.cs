namespace Cron.Extensions.Expressions;

/// <summary>
/// Provides extension methods for executing and evaluating cron expressions.
/// </summary>
public static class ExecutionExtensions
{
    private static readonly string _wildcard = "*";

    /// <summary>
    /// The default value for <see cref="GetNextExecution"/>'s <c>maxSearchYears</c> parameter.
    /// </summary>
    internal const int DefaultMaxSearchYears = 10;

    /// <summary>
    /// Get the next execution time of the cron expression from the start date. If no start date is provided, the current date is used.
    /// </summary>
    /// <remarks>
    /// Time arithmetic uses <see cref="DateTime"/> addition, which does not apply daylight saving time (DST) rules.
    /// When <paramref name="start"/> has <see cref="DateTime.Kind"/> <see cref="DateTimeKind.Local"/> and the next run
    /// falls in a skipped hour (e.g. 2:00 AM on spring-forward day), the returned time may be invalid in the local zone.
    /// When it falls in a repeated hour (e.g. 1:30 AM on fall-back day), the result may be ambiguous.
    /// For predictable behavior across DST boundaries, use a <paramref name="start"/> with <see cref="DateTimeKind.Utc"/>.
    /// Following standard cron semantics, when both <see cref="CronExpression.Day"/> and <see cref="CronExpression.DayOfWeek"/>
    /// are restricted (neither is <c>*</c>), a date matches if either field matches, not only when both do.
    /// </remarks>
    /// <param name="expression">The cron expression to evaluate.</param>
    /// <param name="start">The date and time to evaluate from. When <c>null</c>, the current local time is used.</param>
    /// <param name="maxSearchYears">
    /// The maximum number of years past <paramref name="start"/> the search will look before giving up. Defaults to
    /// <see cref="DefaultMaxSearchYears"/>, which comfortably covers every legitimately satisfiable expression -
    /// including the rarest case, a day 29 combined with February, which requires a leap year. This guard always
    /// applies; there is no way to disable it, since reaching it should always indicate a defect rather than a
    /// slow-but-valid search.
    /// </param>
    /// <returns>The next scheduled execution time for the specified cron expression.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxSearchYears"/> is less than 1, or when no value satisfying
    /// <see cref="CronExpression.Day"/> can ever occur in any month satisfying <see cref="CronExpression.Month"/> -
    /// the same check <see cref="CronExpression.ToCronExpression"/> performs.
    /// </exception>
    /// <exception cref="CronSearchHorizonExceededException">
    /// Thrown when no matching execution time is found within <paramref name="maxSearchYears"/> of
    /// <paramref name="start"/>. This should not happen for any expression that passes the day/month check above;
    /// seeing it most likely indicates a defect in the search itself.
    /// </exception>
    public static DateTime GetNextExecution(this CronExpression expression, DateTime? start = null, int maxSearchYears = DefaultMaxSearchYears)
    {
        if (maxSearchYears < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSearchYears), $"{nameof(maxSearchYears)} must be at least 1.");
        }

        FieldValidator.ValidateDayOfMonth(expression.Day, expression.Month);

        var now = DateTime.Now;
        var next = start ?? now;
        start ??= now;

        var horizon = start.Value.AddYears(maxSearchYears);

        while (true)
        {
            if (next > horizon)
            {
                throw new CronSearchHorizonExceededException(expression, start.Value, maxSearchYears);
            }

            if (!CanExecute(next.Minute, expression.Minute, Units.Minute))
            {
                next = next
                    .AddMinutes(1);
                continue;
            }

            if (!CanExecute(next.Hour, expression.Hour, Units.Hour))
            {
                next = next
                    .AddMinutes(-next.Minute)
                    .AddHours(1);
                continue;
            }

            if (!MatchesDayFields(next.Day, (int)next.DayOfWeek, expression.Day, expression.DayOfWeek))
            {
                next = next
                    .AddMinutes(-next.Minute)
                    .AddHours(-next.Hour)
                    .AddDays(1);
                continue;
            }

            if (!CanExecute(next.Month, expression.Month, Units.Month))
            {
                next = next
                    .AddMinutes(-next.Minute)
                    .AddHours(-next.Hour)
                    .AddDays(-next.Day + 1)
                    .AddMonths(1);
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
    /// <remarks>
    /// Following standard cron semantics, when both <see cref="CronExpression.Day"/> and <see cref="CronExpression.DayOfWeek"/>
    /// are restricted (neither is <c>*</c>), the date matches if either field matches, not only when both do.
    /// </remarks>
    /// <param name="expression">The cron expression to evaluate.</param>
    /// <param name="date">The date and time to test against the cron expression.</param>
    /// <returns><c>true</c> when the cron expression matches the provided date and time; otherwise, <c>false</c>.</returns>
    public static bool WillRunOn(this CronExpression expression, DateTime date)
    {
        return CanExecute(date.Minute, expression.Minute, Units.Minute)
               && CanExecute(date.Hour, expression.Hour, Units.Hour)
               && MatchesDayFields(date.Day, (int)date.DayOfWeek, expression.Day, expression.DayOfWeek)
               && CanExecute(date.Month, expression.Month, Units.Month);
    }

    /// <summary>
    /// Determines whether the day-of-month and day-of-week fields, taken together, match the given date.
    /// </summary>
    /// <remarks>
    /// Following standard (Vixie/Kubernetes) cron semantics: when both fields are restricted (neither is
    /// <c>*</c>), the date matches if <em>either</em> field matches. When at most one field is restricted,
    /// both must match - which is equivalent to requiring only the restricted field, since an unrestricted
    /// <c>*</c> field always matches.
    /// </remarks>
    private static bool MatchesDayFields(int day, int dayOfWeek, string dayExpression, string dayOfWeekExpression)
    {
        var dayRestricted = dayExpression != _wildcard;
        var dayOfWeekRestricted = dayOfWeekExpression != _wildcard;

        var dayMatches = CanExecute(day, dayExpression, Units.Day);
        var dayOfWeekMatches = MatchesDayOfWeek(dayOfWeek, dayOfWeekExpression);

        return (dayRestricted && dayOfWeekRestricted)
            ? dayMatches || dayOfWeekMatches
            : dayMatches && dayOfWeekMatches;
    }

    /// <summary>
    /// Determines whether an actual day-of-week value matches a <see cref="CronExpression.DayOfWeek"/>
    /// expression, treating <c>0</c> and <c>7</c> as equally valid representations of Sunday.
    /// </summary>
    /// <remarks>
    /// <see cref="DateTime.DayOfWeek"/> never reports <c>7</c> - Sunday is always <c>0</c> - but the
    /// expression itself may spell Sunday as <c>7</c>, whether alone (<c>"7"</c>), in a list (<c>"1,7"</c>),
    /// or as the end of a range (<c>"5-7"</c>, meaning Friday through Sunday). Checking the actual Sunday
    /// value against both <c>0</c> and <c>7</c> handles every one of those shapes uniformly, without the
    /// matcher needing to know which syntax was used.
    /// </remarks>
    private static bool MatchesDayOfWeek(int dayOfWeek, string dayOfWeekExpression)
    {
        if (CanExecute(dayOfWeek, dayOfWeekExpression, Units.DayOfWeek)) return true;

        return dayOfWeek == 0 && CanExecute(7, dayOfWeekExpression, Units.DayOfWeek);
    }

    private static bool CanExecute(int value, string expression, Units unit)
    {
        if (expression == _wildcard) return true;

        if (expression.Contains(','))
        {
            var values = expression.Split(',');
            return values.Any(v => CanExecute(value, v, unit));
        }

        if (expression.Contains('-'))
        {
            var values = expression.Split('-');
            return value >= int.Parse(values[0]) && value <= int.Parse(values[1]);
        }

        if (expression.Contains('/'))
        {
            var values = expression.Split('/');
            var start = (values[0] == _wildcard) ? FieldValidator.GetMinValue(unit) : int.Parse(values[0]);
            var step = int.Parse(values[1]);

            return value >= start && (value - start) % step == 0;
        }

        return value == int.Parse(expression);
    }
}
