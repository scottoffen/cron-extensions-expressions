namespace Cron.Extensions.Expressions;

internal static class FieldValidator
{
    private static readonly string _wildcard = "*";

    private static readonly Dictionary<Units, (int min, int max)> _limits = new()
    {
        { Units.Minute, (0, 59) },
        { Units.Hour, (0, 23) },
        { Units.Day, (1, 31) },
        { Units.Month, (1, 12) },
        { Units.DayOfWeek, (0, 6) }
    };

    private static readonly Dictionary<int, int> _daysInMonths = new()
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

    public static int GetMinValue(Units unit) => _limits[unit].min;

    public static int GetMaxValue(Units unit) => _limits[unit].max;

    public static void Validate(string value, Units unit)
    {
        if (value == _wildcard) return;

        if (value.Contains(','))
        {
            var values = value.Split(',');
            foreach (var v in values)
            {
                Validate(v, unit);
            }
        }
        else if (value.Contains('-'))
        {
            var values = value.Split('-', 2);
            var start = int.Parse(values[0]);
            var end = int.Parse(values[1]);

            Validate(start, unit);
            Validate(end, unit);

            if (start >= end)
            {
                throw new ArgumentOutOfRangeException(unit.ToString(), $"Start value {start} for {unit.ToString().ToLower()} must be less than end value {end}.");
            }
        }
        else if (value.Contains('/'))
        {
            if (unit == Units.DayOfWeek)
            {
                throw new NotSupportedException("Interval values are not supported for day of week.");
            }

            var values = value.Split('/');
            var start = (values[0] == _wildcard)
                ? -1
                : int.Parse(values[0]);

            var interval = int.Parse(values[1]);

            if (start >= 0) Validate(start, unit);
            Validate(interval, unit);

            if (interval < 1)
            {
                throw new ArgumentOutOfRangeException(unit.ToString(), $"Interval value {interval} for {unit.ToString().ToLower()} must be greater than 0.");
            }
        }
        else
        {
            var v = int.Parse(value);
            Validate(v, unit);
        }
    }

    public static void Validate(int value, Units unit)
    {
        if (value < _limits[unit].min || value > _limits[unit].max)
        {
            throw new ArgumentOutOfRangeException(unit.ToString(), $"Value {value} for {unit.ToString().ToLower()} must be between {GetMinValue(unit)} and {GetMaxValue(unit)}.");
        }
    }

    public static void ValidateDayOfMonth(string day, string month)
    {
        if (int.TryParse(day, out var dayOfMonth) && int.TryParse(month, out var monthOfYear))
        {
            if (dayOfMonth < 1 || dayOfMonth > _daysInMonths[monthOfYear])
            {
                throw new ArgumentOutOfRangeException(nameof(CronExpression.Month), $"Day of month {dayOfMonth} for month {monthOfYear} is invalid.");
            }
        }
    }
}
