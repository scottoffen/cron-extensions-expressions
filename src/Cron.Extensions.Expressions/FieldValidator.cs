namespace Cron.Extensions.Expressions;

internal static class FieldValidator
{
    private static readonly char[] _rangeSeparator = ['-'];
    private static readonly char[] _stepSeparator = ['/'];
    private static readonly char[] _listSeparator = [','];

    private struct MinMax
    {
        internal readonly int Min;
        internal readonly int Max;
        internal MinMax(int min, int max) { Min = min; Max = max; }
    }

    private static readonly string _wildcard = "*";

    private static readonly Dictionary<Units, MinMax> _limits = new Dictionary<Units, MinMax>
    {
        { Units.Minute, new MinMax(0, 59) },
        { Units.Hour, new MinMax(0, 23) },
        { Units.Day, new MinMax(1, 31) },
        { Units.Month, new MinMax(1, 12) },
        { Units.DayOfWeek, new MinMax(0, 7) } // Per the cron specification, both 0 and 7 represent Sunday.
    };

    private static readonly Dictionary<int, int> _daysInMonths = new Dictionary<int, int>
    {
        { 1, 31 },
        { 2, 29 },
        { 3, 31 },
        { 4, 30 },
        { 5, 31 },
        { 6, 30 },
        { 7, 31 },
        { 8, 31 },
        { 9, 30 },
        { 10, 31 },
        { 11, 30 },
        { 12, 31 }
    };

    public static int GetMinValue(Units unit) => _limits[unit].Min;

    public static int GetMaxValue(Units unit) => _limits[unit].Max;

    public static void Validate(string value, Units unit)
    {
        if (value == _wildcard) return;

        if (value.Contains(','))
            ValidateCommaSeparated(value, unit);
        else if (value.Contains('-'))
            ValidateRange(value, unit);
        else if (value.Contains('/'))
            ValidateStep(value, unit);
        else
            ValidateSingle(value, unit);
    }

    public static void Validate(int value, Units unit)
    {
        if (value < _limits[unit].Min || value > _limits[unit].Max)
        {
            throw new ArgumentOutOfRangeException(unit.ToString(), $"Value {value} for {unit.ToString().ToLower()} must be between {GetMinValue(unit)} and {GetMaxValue(unit)}.");
        }
    }

    private static void ValidateCommaSeparated(string value, Units unit)
    {
        var values = value.Split(',');
        foreach (var v in values)
            Validate(v, unit);
    }

    private static void ValidateRange(string value, Units unit)
    {
        var values = value.Split(_rangeSeparator, 2);
        var start = int.Parse(values[0]);
        var end = int.Parse(values[1]);

        Validate(start, unit);
        Validate(end, unit);

        if (start >= end)
            throw new ArgumentOutOfRangeException(unit.ToString(), $"Start value {start} for {unit.ToString().ToLower()} must be less than end value {end}.");
    }

    private static void ValidateStep(string value, Units unit)
    {
        if (unit == Units.DayOfWeek)
            throw new NotSupportedException("Interval values are not supported for day of week.");

        var values = value.Split(_stepSeparator, 2);
        var start = (values[0] == _wildcard) ? -1 : int.Parse(values[0]);
        var interval = int.Parse(values[1]);

        if (start >= 0) Validate(start, unit);

        // Check the interval-specific rule before the generic field-range check. An interval
        // is a step size, not a field value, so a zero or negative interval should report
        // that directly. Otherwise the 1-based fields (day, month) would fail the range check
        // first and describe the interval as though it were a day or month value.
        if (interval < 1)
            throw new ArgumentOutOfRangeException(unit.ToString(), $"Interval value {interval} for {unit.ToString().ToLower()} must be greater than 0.");

        Validate(interval, unit);
    }

    private static void ValidateSingle(string value, Units unit)
    {
        var v = int.Parse(value);
        Validate(v, unit);
    }

    public static void ValidateDayOfMonth(string day, string month)
    {
        if (day == _wildcard) return;

        var days = ExpandExpression(day, Units.Day);
        var months = ExpandExpression(month, Units.Month);

        foreach (var monthOfYear in months)
        {
            var maxDay = _daysInMonths[monthOfYear];
            if (days.Any(d => d <= maxDay)) return;
        }

        var message = days.Length == 1
            ? $"Day of month {days[0]} is invalid for months: {string.Join(",", months)}."
            : $"No value in day expression '{day}' is valid for any of the selected months: {string.Join(",", months)}.";

        throw new ArgumentOutOfRangeException(nameof(CronExpression.Day), message);
    }

    /// <summary>
    /// Expands a field expression (wildcard, single value, list, range, and/or step - any
    /// combination the property setters already accept) into the concrete set of values it
    /// represents, honoring <paramref name="unit"/>'s own minimum and maximum.
    /// </summary>
    /// <remarks>
    /// This is the same expansion <see cref="Units.Month"/> has always used for the day/month
    /// cross-check in <see cref="ValidateDayOfMonth"/>; it is now unit-generic so the same logic
    /// covers <see cref="Units.Day"/> too, closing a gap where a <c>Day</c> expressed as
    /// anything other than a bare single value (a list, range, or step) previously skipped that
    /// check entirely.
    /// </remarks>
    private static int[] ExpandExpression(string expression, Units unit)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException($"{unit} expression must not be empty.", unit.ToString());

        var values = new SortedSet<int>();
        var segments = expression.Split(_listSeparator, StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            ExpandSegment(segment.Trim(), unit, values);
        }

        return values.ToArray();
    }

    private static void ExpandSegment(string segment, Units unit, SortedSet<int> values)
    {
        var stepParts = segment.Split(_stepSeparator, StringSplitOptions.RemoveEmptyEntries);

        if (stepParts.Length > 2)
        {
            throw new ArgumentException($"Invalid {unit} expression segment '{segment}'.", unit.ToString());
        }

        var rangePart = stepParts[0].Trim();
        var hasStep = stepParts.Length == 2;
        var step = 1;

        if (hasStep &&
            (!int.TryParse(stepParts[1].Trim(), out step) || step < 1))
        {
            throw new ArgumentOutOfRangeException(nameof(segment), $"Invalid step value in {unit} expression segment '{segment}'.");
        }

        int start, end;
        ParseRange(rangePart, unit, out start, out end);

        // An "N/M" segment (a bare starting value with a step, e.g. "2/5") expands from N
        // through the field's maximum every M steps - it is not just the single value N.
        // A wildcard ("*/M") and an explicit range ("A-B/M") already carry the correct
        // end value from ParseRange and are left untouched.
        if (hasStep && rangePart != _wildcard && !rangePart.Contains('-'))
        {
            end = GetMaxValue(unit);
        }

        for (var value = start; value <= end; value += step)
        {
            values.Add(value);
        }
    }

    private static void ParseRange(string rangePart, Units unit, out int start, out int end)
    {
        if (rangePart == _wildcard)
        {
            start = GetMinValue(unit);
            end = GetMaxValue(unit);
            return;
        }

        // Try the whole segment as one value first. This must come before splitting on '-',
        // since a leading '-' is a negative sign here, not a range separator - splitting
        // "-1" on '-' with empty entries removed silently collapses it to "1", losing the
        // sign entirely rather than rejecting it as out of range.
        if (int.TryParse(rangePart, out var singleValue))
        {
            Validate(singleValue, unit);
            start = end = singleValue;
            return;
        }

        var rangeParts = rangePart.Split(_rangeSeparator, 2);

        if (rangeParts.Length == 2 && rangeParts[0].Length > 0)
        {
            start = ParseFieldValue(rangeParts[0].Trim(), unit);
            end = ParseFieldValue(rangeParts[1].Trim(), unit);

            if (start > end)
            {
                throw new ArgumentOutOfRangeException(nameof(rangePart), $"Invalid {unit} range '{rangePart}'.");
            }
            return;
        }

        throw new ArgumentException($"Invalid {unit} range '{rangePart}'.", unit.ToString());
    }

    private static int ParseFieldValue(string value, Units unit)
    {
        var parsed = int.Parse(value);
        Validate(parsed, unit);
        return parsed;
    }
}
