using System.Diagnostics.CodeAnalysis;

namespace Cron.Extensions.Expressions;

/// <summary>
/// Represents a Kubernetes-supported cron expression.
/// </summary>
public class CronExpression
{
    private string _minute = "*";
    private string _hour = "*";
    private string _day = "*";
    private string _month = "*";
    private string _dayOfWeek = "*";

    public CronExpression() { }

    public CronExpression(string? minute = null, string? hour = null, string? day = null, string? month = null, string? dayOfWeek = null) : this()
    {
        Minute = minute ?? "*";
        Hour = hour ?? "*";
        Day = day ?? "*";
        Month = month ?? "*";
        DayOfWeek = dayOfWeek ?? "*";
    }

    /// <summary>
    /// Gets or sets the minute component of the cron expression.
    /// </summary>
    /// <exception cref="FormatException">Thrown when the value is not a valid cron expression.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the values are out of range for a valid minute expression.</exception>
    public string Minute
    {
        get { return _minute; }
        set
        {
            FieldValidator.Validate(value, Units.Minute);
            _minute = value;
        }
    }

    /// <summary>
    /// Gets or sets the hour component of the cron expression.
    /// </summary>
    /// <exception cref="FormatException">Thrown when the value is not a valid cron expression.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the values are out of range for a valid hour expression.</exception>
    public string Hour
    {
        get { return _hour; }
        set
        {
            FieldValidator.Validate(value, Units.Hour);
            _hour = value;
        }
    }

    /// <summary>
    /// Gets or sets the day component of the cron expression.
    /// </summary>
    /// <exception cref="FormatException">Thrown when the value is not a valid cron expression.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the values are out of range for a valid day expression.</exception>
    public string Day
    {
        get { return _day; }
        set
        {
            FieldValidator.Validate(value, Units.Day);
            _day = value;
        }
    }

    /// <summary>
    /// Gets or sets the month component of the cron expression.
    /// </summary>
    /// <exception cref="FormatException">Thrown when the value is not a valid cron expression.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the values are out of range for a valid month expression.</exception>
    public string Month
    {
        get { return _month; }
        set
        {
            FieldValidator.Validate(value, Units.Month);
            _month = value;
        }
    }

    /// <summary>
    /// Gets or sets the day of week component of the cron expression.
    /// </summary>
    /// <exception cref="FormatException">Thrown when the value is not a valid cron expression.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the values are out of range for a valid day of week expression.</exception>
    public string DayOfWeek
    {
        get { return _dayOfWeek; }
        set
        {
            FieldValidator.Validate(value, Units.DayOfWeek);
            _dayOfWeek = value;
        }
    }

    /// <summary>
    /// Converts the current instance to a cron expression.
    /// </summary>
    /// <remarks>An <see cref="ArgumentOutOfRangeException"/> will be thrown if there is an explicit day and month in the cron expression, and the day is not valid for the month. E.g. February 30.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the day of the month is not valid for the specified month when an explicit day and month are provided.</exception>
    public string ToCronExpression()
    {
        FieldValidator.ValidateDayOfMonth(_day, _month);
        return $"{_minute} {_hour} {_day} {_month} {_dayOfWeek}";
    }

    /// <summary>
    /// Parses a cron expression into a <see cref="CronExpression"/> instance.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="FormatException"></exception>
    public static CronExpression Parse(string value)
    {
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
        {
            throw new FormatException($"Invalid cron expression. Found {parts.Length} parts instead of 5.");
        }

        return new CronExpression(parts[0], parts[1], parts[2], parts[3], parts[4]);
    }

    /// <summary>
    /// Tries to parse a cron expression into a <see cref="CronExpression"/> instance.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static bool TryParse(string value, [NotNullWhen(true)] out CronExpression? expression)
    {
        try
        {
            expression = Parse(value);
            return true;
        }
        catch
        {
            expression = null;
            return false;
        }
    }
}
