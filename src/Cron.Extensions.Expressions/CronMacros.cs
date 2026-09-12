namespace Cron.Extensions.Expressions;

/// <summary>
/// Expands the standard crontab macros into the five-field expression they are shorthand for.
/// </summary>
internal static class CronMacros
{
    private static readonly Dictionary<string, string> _macros = new(StringComparer.OrdinalIgnoreCase)
    {
        { "@yearly", "0 0 1 1 *" },
        { "@annually", "0 0 1 1 *" },
        { "@monthly", "0 0 1 * *" },
        { "@weekly", "0 0 * * 0" },
        { "@daily", "0 0 * * *" },
        { "@midnight", "0 0 * * *" },
        { "@hourly", "0 * * * *" }
    };

    private const string Reboot = "@reboot";

    /// <summary>
    /// Expands <paramref name="value"/> to its five-field equivalent if, once trimmed, it
    /// exactly matches a recognized macro (case-insensitively); otherwise returns it unchanged.
    /// </summary>
    /// <remarks>
    /// <c>@reboot</c> is a special case: it is a real crontab macro, unlike an outright typo such
    /// as <c>@foo</c>, but it has no five-field schedule to expand into - it means "once, when
    /// the scheduler starts," not a recurring time. Rather than let it fall through to the same
    /// generic "wrong number of fields" failure an unrecognized token would produce,
    /// <c>@reboot</c> is recognized specifically so it can fail with a message that explains why,
    /// the same way step syntax on <see cref="Units.DayOfWeek"/> gets its own explanatory
    /// <see cref="NotSupportedException"/> instead of a generic one.
    /// </remarks>
    /// <exception cref="NotSupportedException">Thrown when the trimmed value is <c>@reboot</c>.</exception>
    public static string Expand(string value)
    {
        var trimmed = value.Trim();

        if (string.Equals(trimmed, Reboot, StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(
                "The \"@reboot\" macro is not supported. It means \"once, when the scheduler starts,\" " +
                "which is not a recurring time and has no five-field cron schedule to expand into.");
        }

        return _macros.TryGetValue(trimmed, out var expansion) ? expansion : value;
    }
}
