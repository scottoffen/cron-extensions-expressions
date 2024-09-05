namespace Cron.Extensions.Expressions.Tests;

public class RangeExtensionsTests
{
    [Fact]
    public void RangeOfMinutesTest()
    {
        var start = Random.Shared.Next(0, 30);
        var end = Random.Shared.Next(30, 60);
        var expression = new CronExpression();

        Should.Throw<ArgumentException>(() => expression.RangeOfMinutes(end, start));

        expression.RangeOfMinutes(start, end);
        expression.Minute.ShouldBe($"{start}-{end}");
    }

    [Fact]
    public void RangeOfHoursTest()
    {
        var start = Random.Shared.Next(0, 12);
        var end = Random.Shared.Next(12, 24);
        var expression = new CronExpression();

        Should.Throw<ArgumentException>(() => expression.RangeOfHours(end, start));

        expression.RangeOfHours(start, end);
        expression.Hour.ShouldBe($"{start}-{end}");
    }

    [Fact]
    public void RangeOfDaysTest()
    {
        var start = Random.Shared.Next(1, 15);
        var end = Random.Shared.Next(15, 32);
        var expression = new CronExpression();

        Should.Throw<ArgumentException>(() => expression.RangeOfDays(end, start));

        expression.RangeOfDays(start, end);
        expression.Day.ShouldBe($"{start}-{end}");
    }

    [Fact]
    public void RangeOfMonthsTest()
    {
        var start = Random.Shared.Next(1, 6);
        var end = Random.Shared.Next(6, 13);
        var expression = new CronExpression();

        Should.Throw<ArgumentException>(() => expression.RangeOfMonths(end, start));

        expression.RangeOfMonths(start, end);
        expression.Month.ShouldBe($"{start}-{end}");
    }

    [Fact]
    public void RangeOfWeekTest()
    {
        var start = Random.Shared.Next(0, 3);
        var end = Random.Shared.Next(3, 7);
        var expression = new CronExpression();

        Should.Throw<ArgumentException>(() => expression.RangeOfWeek(end, start));

        expression.RangeOfWeek(start, end);
        expression.DayOfWeek.ShouldBe($"{start}-{end}");
    }
}
