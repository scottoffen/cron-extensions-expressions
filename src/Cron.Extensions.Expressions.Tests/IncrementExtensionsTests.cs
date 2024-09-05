namespace Cron.Extensions.Expressions.Tests;

public class IncrementExtensionsTests
{
    [Fact]
    public void EveryMinuteTest()
    {
        var minute = Random.Shared.Next(2, 60);

        var expression = new CronExpression(minute: "15");
        expression.Minute.ShouldBe("15");

        expression.EveryMinute();
        expression.Minute.ShouldBe("*");

        expression.EveryXMinutes(minute);
        expression.Minute.ShouldBe($"*/{minute}");

        expression.EveryXMinutes(1);
        expression.Minute.ShouldBe("*");

        expression.EveryXMinutes(1, 2);
        expression.Minute.ShouldBe("1/2");

        expression.EveryXMinutes(1, 1);
        expression.Minute.ShouldBe("*");
    }

    [Fact]
    public void EveryHourTest()
    {
        var hour = Random.Shared.Next(2, 24);

        var expression = new CronExpression(hour: "6");
        expression.Hour.ShouldBe("6");

        expression.EveryHour();
        expression.Hour.ShouldBe("*");

        expression.EveryXHours(hour);
        expression.Hour.ShouldBe($"*/{hour}");

        expression.EveryXHours(1);
        expression.Hour.ShouldBe("*");

        expression.EveryXHours(1, 2);
        expression.Hour.ShouldBe("1/2");

        expression.EveryXHours(1, 1);
        expression.Hour.ShouldBe("*");
    }

    [Fact]
    public void EveryDayTest()
    {
        var day = Random.Shared.Next(2, 31);

        var expression = new CronExpression(day: "15");
        expression.Day.ShouldBe("15");

        expression.EveryDay();
        expression.Day.ShouldBe("*");

        expression.EveryXDays(day);
        expression.Day.ShouldBe($"*/{day}");

        expression.EveryXDays(1);
        expression.Day.ShouldBe("*");

        expression.EveryXDays(1, 2);
        expression.Day.ShouldBe("1/2");

        expression.EveryXDays(1, 1);
        expression.Day.ShouldBe("*");
    }

    [Fact]
    public void EveryMonthTest()
    {
        var month = Random.Shared.Next(2, 13);

        var expression = new CronExpression(month: "6");
        expression.Month.ShouldBe("6");

        expression.EveryMonth();
        expression.Month.ShouldBe("*");

        expression.EveryXMonths(month);
        expression.Month.ShouldBe($"*/{month}");

        expression.EveryXMonths(1);
        expression.Month.ShouldBe("*");

        expression.EveryXMonths(1, 2);
        expression.Month.ShouldBe("1/2");

        expression.EveryXMonths(1, 1);
        expression.Month.ShouldBe("*");
    }
}

