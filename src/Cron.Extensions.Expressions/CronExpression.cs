using System.Diagnostics.CodeAnalysis;

namespace Cron.Extensions.Expressions;

/// <summary>
/// Represents a Kubernetes-supported cron expression.
/// </summary>
public sealed class CronExpression
{
    private static readonly char[] _separators = [' '];

    private string _minute = "*";
    private string _hour = "*";
    private string _day = "*";
    private string _month = "*";
    private string _dayOfWeek = "*";

    /// <summary>
    /// Initializes a new instance of the <see cref="CronExpression"/> class with the specified values.
    /// </summary>
    /// <remarks>
    /// Any component not provided defaults to "*", meaning "every" for that component in cron syntax.
    /// </remarks>
    /// <param name="minute">The minute component (0-59, or cron syntax). Defaults to "*".</param>
    /// <param name="hour">The hour component (0-23, or cron syntax). Defaults to "*".</param>
    /// <param name="day">The day of month component (1-31, or cron syntax). Defaults to "*".</param>
    /// <param name="month">The month component (1-12, or cron syntax). Defaults to "*".</param>
    /// <param name="dayOfWeek">The day of week component (0-7, where both 0 and 7 represent Sunday, or cron syntax). Defaults to "*".</param>
    public CronExpression(string? minute = null, string? hour = null, string? day = null, string? month = null, string? dayOfWeek = null)
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
    /// <remarks>
    /// Per the cron specification, <c>7</c> is accepted as an alias for Sunday and is stored
    /// and reflected exactly as assigned - no normalization occurs. <c>7</c> remains a distinct
    /// value through validation, storage, and formatting, which is what allows a range like
    /// <c>"5-7"</c> to mean Friday through Sunday rather than being rejected. Matching (see
    /// <see cref="ExecutionExtensions"/>) is where <c>0</c> and <c>7</c> are treated as
    /// equivalent: an actual Sunday satisfies either representation, alone or within a list or
    /// range.
    /// </remarks>
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
    /// Converts the current instance to a cron expression string.
    /// </summary>
    /// <returns>A five-part cron expression string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an explicit day is not valid for the specified month (e.g., February 30).</exception>
    public string ToCronExpression()
    {
        FieldValidator.ValidateDayOfMonth(_day, _month);
        return $"{_minute} {_hour} {_day} {_month} {_dayOfWeek}";
    }

    /// <summary>
    /// Parses a cron expression string into a <see cref="CronExpression"/> instance.
    /// </summary>
    /// <param name="value">The five-part cron expression string to parse.</param>
    /// <returns>A new <see cref="CronExpression"/> instance.</returns>
    /// <exception cref="FormatException">Thrown when the value does not contain exactly five space-separated parts.</exception>
    public static CronExpression Parse(string value)
    {
        var parts = value.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
        {
            throw new FormatException($"Invalid cron expression. Found {parts.Length} parts instead of 5.");
        }

        return new CronExpression(parts[0], parts[1], parts[2], parts[3], parts[4]);
    }

    /// <summary>
    /// Attempts to parse a cron expression string into a <see cref="CronExpression"/> instance.
    /// </summary>
    /// <param name="value">The five-part cron expression string to parse.</param>
    /// <param name="expression">When this method returns, contains the parsed expression if successful; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the value was successfully parsed; otherwise, <c>false</c>.</returns>
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
