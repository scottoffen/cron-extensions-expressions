namespace Cron.Extensions.Expressions;

/// <summary>
/// Translates the three-letter month and day-of-week names this library accepts into the
/// numeric values <see cref="FieldValidator"/> and <see cref="ExecutionExtensions"/> already
/// understand, so neither of those needs to know names exist at all.
/// </summary>
internal static class FieldNames
{
    private static readonly char[] _listSeparator = [','];
    private static readonly char[] _stepSeparator = ['/'];
    private static readonly char[] _rangeSeparator = ['-'];

    private static readonly Dictionary<string, string> _monthNames = new(StringComparer.OrdinalIgnoreCase)
    {
        { "JAN", "1" }, { "FEB", "2" }, { "MAR", "3" }, { "APR", "4" },
        { "MAY", "5" }, { "JUN", "6" }, { "JUL", "7" }, { "AUG", "8" },
        { "SEP", "9" }, { "OCT", "10" }, { "NOV", "11" }, { "DEC", "12" }
    };

    private static readonly Dictionary<string, string> _dayOfWeekNames = new(StringComparer.OrdinalIgnoreCase)
    {
        { "SUN", "0" }, { "MON", "1" }, { "TUE", "2" }, { "WED", "3" },
        { "THU", "4" }, { "FRI", "5" }, { "SAT", "6" }
    };

    /// <summary>
    /// Replaces any recognized name in <paramref name="value"/> with its numeric equivalent.
    /// Digits, <c>*</c>, <c>,</c>, <c>-</c>, and <c>/</c> all pass through untouched, and units
    /// other than <see cref="Units.Month"/> and <see cref="Units.DayOfWeek"/> have no names to
    /// translate in the first place, so the value is returned exactly as given.
    /// </summary>
    /// <remarks>
    /// Only a value position - a bare value, or either side of a range - is eligible for a name.
    /// The interval side of a step is always numeric, matching every cron dialect that supports
    /// names at all: <c>"JAN/3"</c> is left with <c>JAN</c> untranslated, so <see cref="FieldValidator"/>
    /// rejects it as malformed on its own terms, the same way it would reject any other
    /// non-numeric interval.
    /// </remarks>
    public static string Translate(string value, Units unit)
    {
        var names = unit switch
        {
            Units.Month => _monthNames,
            Units.DayOfWeek => _dayOfWeekNames,
            _ => null
        };

        if (names is null || value == "*") return value;

        var segments = value.Split(_listSeparator);
        for (var i = 0; i < segments.Length; i++)
        {
            segments[i] = TranslateSegment(segments[i], names);
        }

        return string.Join(",", segments);
    }

    private static string TranslateSegment(string segment, Dictionary<string, string> names)
    {
        var stepParts = segment.Split(_stepSeparator, 2);
        var rangePart = TranslateRangePart(stepParts[0], names);

        return stepParts.Length == 2
            ? $"{rangePart}/{stepParts[1]}"
            : rangePart;
    }

    private static string TranslateRangePart(string rangePart, Dictionary<string, string> names)
    {
        if (rangePart == "*") return rangePart;

        var bounds = rangePart.Split(_rangeSeparator, 2);
        for (var i = 0; i < bounds.Length; i++)
        {
            if (names.TryGetValue(bounds[i], out var numeric))
            {
                bounds[i] = numeric;
            }
        }

        return string.Join("-", bounds);
    }
}
