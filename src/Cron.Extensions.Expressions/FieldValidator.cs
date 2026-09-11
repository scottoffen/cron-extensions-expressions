namespace Cron.Extensions.Expressions;

internal static class FieldValidator
{
    private static readonly char[] _rangeSeparator = ['-'];
    private static readonly char[] _stepSeparator = ['/'];
    private static readonly char[] _monthSeparator = [','];

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
        Validate(interval, unit);

        if (interval < 1)
            throw new ArgumentOutOfRangeException(unit.ToString(), $"Interval value {interval} for {unit.ToString().ToLower()} must be greater than 0.");
    }

    private static void ValidateSingle(string value, Units unit)
    {
        var v = int.Parse(value);
        Validate(v, unit);
    }

    public static void ValidateDayOfMonth(string day, string month)
    {
        if (!int.TryParse(day, out var dayOfMonth))
            return;

        if (dayOfMonth < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(CronExpression.Day),
                $"Day of month {dayOfMonth} must be greater than or equal to 1.");
        }

        var months = ExpandMonthExpression(month);

        var validForAny = false;
        var invalidMonths = new List<int>();

        foreach (var monthOfYear in months)
        {
            if (dayOfMonth <= _daysInMonths[monthOfYear])
            {
                validForAny = true;
            }
            else
            {
                invalidMonths.Add(monthOfYear);
            }
        }

        if (!validForAny)
        {
            throw new ArgumentOutOfRangeException(
                nameof(CronExpression.Day),
                $"Day of month {dayOfMonth} is invalid for months: {string.Join(",", months)}.");
        }
    }

    private static int[] ExpandMonthExpression(string monthExpression)
    {
        if (string.IsNullOrWhiteSpace(monthExpression))
            throw new ArgumentException(nameof(CronExpression.Month));

        var months = new SortedSet<int>();
        var segments = monthExpression.Split(_monthSeparator, StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            ExpandSegment(segment.Trim(), months);
        }

        return months.ToArray();
    }

    private static void ExpandSegment(string segment, SortedSet<int> months)
    {
        var stepParts = segment.Split(_stepSeparator, StringSplitOptions.RemoveEmptyEntries);

        if (stepParts.Length > 2)
        {
            throw new ArgumentException($"Invalid month expression segment '{segment}'.", nameof(CronExpression.Month));
        }

        var rangePart = stepParts[0].Trim();
        var hasStep = stepParts.Length == 2;
        var step = 1;

        if (hasStep &&
            (!int.TryParse(stepParts[1].Trim(), out step) || step < 1))
        {
            throw new ArgumentOutOfRangeException(nameof(segment), $"Invalid step value in month expression segment '{segment}'.");
        }

        int start, end;
        ParseRange(rangePart, out start, out end);

        // An "N/M" segment (a bare starting value with a step, e.g. "2/5") expands from N
        // through the field's maximum every M steps - it is not just the single value N.
        // A wildcard ("*/M") and an explicit range ("A-B/M") already carry the correct
        // end value from ParseRange and are left untouched.
        if (hasStep && rangePart != _wildcard && !rangePart.Contains('-'))
        {
            end = GetMaxValue(Units.Month);
        }

        for (var month = start; month <= end; month += step)
        {
            months.Add(month);
        }
    }

    private static void ParseRange(string rangePart, out int start, out int end)
    {
        if (rangePart == "*")
        {
            start = 1;
            end = 12;
            return;
        }

        var rangeParts = rangePart.Split(_rangeSeparator, StringSplitOptions.RemoveEmptyEntries);

        if (rangeParts.Length == 1)
        {
            var month = ParseMonth(rangeParts[0].Trim());
            start = month;
            end = month;
            return;
        }

        if (rangeParts.Length == 2)
        {
            start = ParseMonth(rangeParts[0].Trim());
            end = ParseMonth(rangeParts[1].Trim());

            if (start > end)
            {
                throw new ArgumentOutOfRangeException(nameof(rangePart), $"Invalid month range '{rangePart}'.");
            }
            return;
        }

        throw new ArgumentException($"Invalid month range '{rangePart}'.", nameof(CronExpression.Month));
    }

    private static int ParseMonth(string value)
    {
        if (!int.TryParse(value, out var month) || month < 1 || month > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Month value '{value}' must be between 1 and 12.");
        }

        return month;
    }
}
