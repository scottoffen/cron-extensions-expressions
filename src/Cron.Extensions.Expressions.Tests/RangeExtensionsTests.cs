namespace Cron.Extensions.Expressions.Tests;

public class RangeExtensionsTests
{
    [Fact]
    public void RangeOfMinutesTest()
    {
        var start = Random.Shared.Next(0, 30);
        var end = Random.Shared.Next(30, 60);
        var expression = new CronExpression();

        Should.Throw<ArgumentOutOfRangeException>(() => expression.RangeOfMinutes(end, start));

        expression.RangeOfMinutes(start, end);
        expression.Minute.ShouldBe($"{start}-{end}");
    }

    [Fact]
    public void RangeOfHoursTest()
    {
        var start = Random.Shared.Next(0, 12);
        var end = Random.Shared.Next(12, 24);
        var expression = new CronExpression();

        Should.Throw<ArgumentOutOfRangeException>(() => expression.RangeOfHours(end, start));

        expression.RangeOfHours(start, end);
        expression.Hour.ShouldBe($"{start}-{end}");
    }

    [Fact]
    public void RangeOfDaysTest()
    {
        var start = Random.Shared.Next(1, 15);
        var end = Random.Shared.Next(15, 32);
        var expression = new CronExpression();

        Should.Throw<ArgumentOutOfRangeException>(() => expression.RangeOfDays(end, start));

        expression.RangeOfDays(start, end);
        expression.Day.ShouldBe($"{start}-{end}");
    }

    [Fact]
    public void RangeOfMonthsTest()
    {
        var start = Random.Shared.Next(1, 6);
        var end = Random.Shared.Next(6, 13);
        var expression = new CronExpression();

        Should.Throw<ArgumentOutOfRangeException>(() => expression.RangeOfMonths(end, start));

        expression.RangeOfMonths(start, end);
        expression.Month.ShouldBe($"{start}-{end}");
    }

    [Fact]
    public void RangeOfWeekTest()
    {
        var start = Random.Shared.Next(0, 3);
        var end = Random.Shared.Next(3, 7);
        var expression = new CronExpression();

        Should.Throw<ArgumentOutOfRangeException>(() => expression.RangeOfWeek(end, start));

        expression.RangeOfWeek(start, end);
        expression.DayOfWeek.ShouldBe($"{start}-{end}");
    }

    [Fact]
    public void RangeOfMonths_WithNames()
    {
        var expression = new CronExpression();

        expression.RangeOfMonths("JAN", "JUN");
        expression.Month.ShouldBe("1-6");
    }

    [Fact]
    public void RangeOfMonths_WithMixedIntAndStringArguments()
    {
        // These overloads exist alongside RangeOfMonths(int,int) and RangeOfMonths(string,string) -
        // none of the four is params-based, so there's no ambiguity risk the way there was for
        // the list methods; each call shape resolves to exactly one overload.
        var expression = new CronExpression();

        expression.RangeOfMonths(1, "JUN");
        expression.Month.ShouldBe("1-6");

        expression.RangeOfMonths("JAN", 6);
        expression.Month.ShouldBe("1-6");
    }

    [Fact]
    public void RangeOfWeek_WithNames()
    {
        var expression = new CronExpression();

        expression.RangeOfWeek("MON", "FRI");
        expression.DayOfWeek.ShouldBe("1-5");
    }

    [Fact]
    public void RangeOfWeek_SunAtEndOfRangeBecomesSeven()
    {
        // SUN as the end of a range means 7, not 0 - a range ending in 0 is always reversed and
        // would always throw, so this can only turn a previously-invalid pattern into a valid one.
        var expression = new CronExpression();

        expression.RangeOfWeek("MON", "SUN");
        expression.DayOfWeek.ShouldBe("1-7");
    }

    [Fact]
    public void RangeOfWeek_WithMixedIntAndStringArguments()
    {
        var expression = new CronExpression();

        expression.RangeOfWeek(1, "SUN");
        expression.DayOfWeek.ShouldBe("1-7");

        expression.RangeOfWeek("MON", 7);
        expression.DayOfWeek.ShouldBe("1-7");

        // An explicit numeric 0 is never reinterpreted as 7 - only the name SUN is
        // context-sensitive, not the digit - so this is still a genuinely reversed range.
        Should.Throw<ArgumentOutOfRangeException>(() => new CronExpression().RangeOfWeek("MON", 0));
    }
}
