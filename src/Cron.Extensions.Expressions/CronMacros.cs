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

    /// <summary>
    /// Expands <paramref name="value"/> to its five-field equivalent if, once trimmed, it
    /// exactly matches a recognized macro (case-insensitively); otherwise returns it unchanged.
    /// </summary>
    /// <remarks>
    /// <c>@reboot</c> is deliberately not included here. Every macro above is shorthand for one
    /// specific, already-representable five-field expression - <c>@reboot</c> is not. It means
    /// "once, when the scheduler starts," which isn't a recurring time at all and has no
    /// five-field form to expand into. Rather than give it special handling for a schedule it
    /// cannot actually express, it is left to fail exactly the way any other unrecognized token
    /// does: unexpanded, it fails <see cref="CronExpression.Parse"/>'s five-part check the same
    /// as <c>@foo</c> or any other typo would.
    /// </remarks>
    public static string Expand(string value)
    {
        var trimmed = value.Trim();
        return _macros.TryGetValue(trimmed, out var expansion) ? expansion : value;
    }
}
