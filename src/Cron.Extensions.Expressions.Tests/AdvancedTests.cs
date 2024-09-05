namespace Cron.Extensions.Expressions.Tests;

public class AdvancedTests
{
    [Fact]
    public void Test1()
    {
        var expected = "*/2 8-17 5-10,15-31 2,4,6,8,10,12 1-5";

        var expression = new CronExpression(day: "5-10,15-31");
        expression
            .EveryXMinutes(2)
            .RangeOfHours(8, 17)
            .OnMonths(2, 4, 6, 8, 10, 12)
            .RangeOfWeek(1, 5);

        var actual = expression.ToCronExpression();

        actual.ShouldBe(expected);
    }
}
