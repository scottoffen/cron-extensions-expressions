namespace Cron.Extensions.Expressions;

/// <summary>
/// Thrown by <see cref="ExecutionExtensions.GetNextExecution"/> when no matching execution time
/// is found within its search horizon.
/// </summary>
/// <remarks>
/// This does not mean the expression is provably unsatisfiable - that case is caught earlier,
/// before the search even begins, as an <see cref="ArgumentOutOfRangeException"/> from the same
/// day/month validation <see cref="CronExpression.ToCronExpression"/> performs. This exception
/// means only that no match was found within the configured number of years, which should not
/// happen for any expression that passed that check - even the rarest legitimately satisfiable
/// case (day 29 combined with February, which requires a leap year) resolves within a handful
/// of years, well inside the default horizon. Seeing this exception in practice most likely
/// indicates a defect in the search itself rather than a problem with the expression.
/// </remarks>
public sealed class CronSearchHorizonExceededException : InvalidOperationException
{
    /// <summary>
    /// The cron expression that was being searched when the horizon was exceeded.
    /// </summary>
    public CronExpression? Expression { get; }

    /// <summary>
    /// The date and time the search started from.
    /// </summary>
    public DateTime Start { get; }

    /// <summary>
    /// The maximum number of years past <see cref="Start"/> the search was willing to look.
    /// </summary>
    public int MaxSearchYears { get; }

    /// <summary>
    /// Creates a new <see cref="CronSearchHorizonExceededException"/> describing the search that exceeded its horizon.
    /// </summary>
    /// <param name="expression">The cron expression that was being searched.</param>
    /// <param name="start">The date and time the search started from.</param>
    /// <param name="maxSearchYears">The maximum number of years past <paramref name="start"/> the search was willing to look.</param>
    public CronSearchHorizonExceededException(CronExpression expression, DateTime start, int maxSearchYears)
        : base($"No matching execution time was found within {maxSearchYears} year(s) of {start:O}.")
    {
        Expression = expression;
        Start = start;
        MaxSearchYears = maxSearchYears;
    }

    /// <summary>
    /// Creates a new <see cref="CronSearchHorizonExceededException"/> with no message.
    /// </summary>
    public CronSearchHorizonExceededException() { }

    /// <summary>
    /// Creates a new <see cref="CronSearchHorizonExceededException"/> with the specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CronSearchHorizonExceededException(string message) : base(message) { }

    /// <summary>
    /// Creates a new <see cref="CronSearchHorizonExceededException"/> with the specified message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    public CronSearchHorizonExceededException(string message, Exception innerException) : base(message, innerException) { }
}
