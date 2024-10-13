namespace Cron.Extensions.Expressions.Tests;

public class CronExpressionTests
{
    [Fact]
    public void ParameterlessConstructorTest()
    {
        var expression = new CronExpression();

        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );
    }

    [Fact]
    public void ConstructorTest()
    {
        var minute = Random.Shared.Next(0, 60).ToString();
        var hour = Random.Shared.Next(0, 24).ToString();
        var day = Random.Shared.Next(1, 32).ToString();
        var month = Random.Shared.Next(1, 13).ToString();
        var dayOfWeek = Random.Shared.Next(0, 7).ToString();

        var expression = new CronExpression(minute, hour, day, month, dayOfWeek);

        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe(minute),
            () => expression.Hour.ShouldBe(hour),
            () => expression.Day.ShouldBe(day),
            () => expression.Month.ShouldBe(month),
            () => expression.DayOfWeek.ShouldBe(dayOfWeek)
        );
    }

    [Fact]
    public void MinutePropertyTest()
    {
        var expression = new CronExpression();

        // Set minute to a single value
        var minute = Random.Shared.Next(0, 60).ToString();
        expression.Minute = minute;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe(minute),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set minute to a range of values
        var minuteRange = $"{Random.Shared.Next(0, 15)}-{Random.Shared.Next(45, 60)}";
        expression.Minute = minuteRange;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe(minuteRange),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set minute to a list of values
        var minuteList = "1,2,3";
        expression.Minute = minuteList;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe(minuteList),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set minute to a step value without start
        var minuteStepEvery = "*/5";
        expression.Minute = minuteStepEvery;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe(minuteStepEvery),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set minute to a step value with start
        var minuteStep = "2/5";
        expression.Minute = minuteStep;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe(minuteStep),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set minute to a wildcard value
        expression.Minute = "*";
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set minute to an invalid value
        expression.ShouldSatisfyAllConditions
        (
            () => Should.Throw<ArgumentOutOfRangeException>(() => expression.Minute = $"45-15"),
            () => Should.Throw<ArgumentOutOfRangeException>(() => expression.Minute = "60"),
            () => Should.Throw<FormatException>(() => expression.Minute = "a"),
            () => Should.Throw<FormatException>(() => expression.Minute = "1a"),
            () => Should.Throw<FormatException>(() => expression.Minute = "*/1a"),
            () => Should.Throw<FormatException>(() => expression.Minute = "1a-2"),
            () => Should.Throw<FormatException>(() => expression.Minute = "1-2a"),
            () => Should.Throw<FormatException>(() => expression.Minute = "a1,2"),
            () => Should.Throw<FormatException>(() => expression.Minute = "1-2a"),
            () => Should.Throw<FormatException>(() => expression.Minute = "1-2-3")
        );
    }

    [Fact]
    public void HourPropertyTest()
    {
        var expression = new CronExpression();

        // Set hour to a single value
        var hour = Random.Shared.Next(0, 24).ToString();
        expression.Hour = hour;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe(hour),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set hour to a range of values
        var hourRange = $"{Random.Shared.Next(0, 12)}-{Random.Shared.Next(12, 24)}";
        expression.Hour = hourRange;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe(hourRange),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set hour to a list of values
        var hourList = "1,2,3";
        expression.Hour = hourList;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe(hourList),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set hour to a step value without start
        var hourStepEvery = "*/5";
        expression.Hour = hourStepEvery;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe(hourStepEvery),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set hour to a step value with start
        var hourStep = "2/5";
        expression.Hour = hourStep;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe(hourStep),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set hour to a wildcard value
        expression.Hour = "*";
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set hour to an invalid value
        expression.ShouldSatisfyAllConditions
        (
            () => Should.Throw<ArgumentOutOfRangeException>(() => expression.Hour = $"15-6"),
            () => Should.Throw<ArgumentOutOfRangeException>(() => expression.Hour = "24"),
            () => Should.Throw<FormatException>(() => expression.Hour = "a"),
            () => Should.Throw<FormatException>(() => expression.Hour = "1a"),
            () => Should.Throw<FormatException>(() => expression.Hour = "*/1a"),
            () => Should.Throw<FormatException>(() => expression.Hour = "1a-2"),
            () => Should.Throw<FormatException>(() => expression.Hour = "1-2a"),
            () => Should.Throw<FormatException>(() => expression.Hour = "a1,2"),
            () => Should.Throw<FormatException>(() => expression.Hour = "1-2a"),
            () => Should.Throw<FormatException>(() => expression.Hour = "1-2-3")
        );
    }

    [Fact]
    public void DayPropertyTest()
    {
        var expression = new CronExpression();

        // Set day to a single value
        var day = Random.Shared.Next(1, 32).ToString();
        expression.Day = day;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe(day),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set day to a range of values
        var dayRange = $"{Random.Shared.Next(1, 16)}-{Random.Shared.Next(16, 32)}";
        expression.Day = dayRange;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe(dayRange),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set day to a list of values
        var dayList = "1,2,3";
        expression.Day = dayList;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe(dayList),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set day to a step value without start
        var dayStepEvery = "*/5";
        expression.Day = dayStepEvery;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe(dayStepEvery),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set day to a step value with start
        var dayStep = "2/5";
        expression.Day = dayStep;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe(dayStep),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set day to a wildcard value
        expression.Day = "*";
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set hour to an invalid value
        expression.ShouldSatisfyAllConditions
        (
            () => Should.Throw<ArgumentOutOfRangeException>(() => expression.Day = $"15-6"),
            () => Should.Throw<ArgumentOutOfRangeException>(() => expression.Day = "32"),
            () => Should.Throw<FormatException>(() => expression.Day = "a"),
            () => Should.Throw<FormatException>(() => expression.Day = "1a"),
            () => Should.Throw<FormatException>(() => expression.Day = "*/1a"),
            () => Should.Throw<FormatException>(() => expression.Day = "1a-2"),
            () => Should.Throw<FormatException>(() => expression.Day = "1-2a"),
            () => Should.Throw<FormatException>(() => expression.Day = "a1,2"),
            () => Should.Throw<FormatException>(() => expression.Day = "1-2a"),
            () => Should.Throw<FormatException>(() => expression.Day = "1-2-3")
        );
    }

    [Fact]
    public void MonthPropertyTest()
    {
        var expression = new CronExpression();

        // Set month to a single value
        var month = Random.Shared.Next(1, 13).ToString();
        expression.Month = month;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe(month),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set month to a range of values
        var monthRange = $"{Random.Shared.Next(1, 7)}-{Random.Shared.Next(7, 13)}";
        expression.Month = monthRange;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe(monthRange),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set month to a list of values
        var monthList = "1,2,3";
        expression.Month = monthList;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe(monthList),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set month to a step value without start
        var monthStepEvery = "*/5";
        expression.Month = monthStepEvery;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe(monthStepEvery),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set month to a step value with start
        var monthStep = "2/5";
        expression.Month = monthStep;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe(monthStep),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set month to a wildcard value
        expression.Month = "*";
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe("*")
        );

        // Set hour to an invalid value
        expression.ShouldSatisfyAllConditions
        (
            () => Should.Throw<ArgumentOutOfRangeException>(() => expression.Month = $"9-6"),
            () => Should.Throw<ArgumentOutOfRangeException>(() => expression.Month = "13"),
            () => Should.Throw<FormatException>(() => expression.Month = "a"),
            () => Should.Throw<FormatException>(() => expression.Month = "1a"),
            () => Should.Throw<FormatException>(() => expression.Month = "*/1a"),
            () => Should.Throw<FormatException>(() => expression.Month = "1a-2"),
            () => Should.Throw<FormatException>(() => expression.Month = "1-2a"),
            () => Should.Throw<FormatException>(() => expression.Month = "a1,2"),
            () => Should.Throw<FormatException>(() => expression.Month = "1-2a"),
            () => Should.Throw<FormatException>(() => expression.Month = "1-2-3")
        );
    }

    [Fact]
    public void DayOfWeekPropertyTest()
    {
        var expression = new CronExpression();

        // Set day of week to a single value
        var dayOfWeek = Random.Shared.Next(0, 7).ToString();
        expression.DayOfWeek = dayOfWeek;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe(dayOfWeek)
        );

        // Set day of week to a range of values
        var dayOfWeekRange = $"{Random.Shared.Next(0, 4)}-{Random.Shared.Next(4, 7)}";
        expression.DayOfWeek = dayOfWeekRange;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe(dayOfWeekRange)
        );

        // Set day of week to a list of values
        var dayOfWeekList = "1,2,3";
        expression.DayOfWeek = dayOfWeekList;
        expression.ShouldSatisfyAllConditions
        (
            () => expression.Minute.ShouldBe("*"),
            () => expression.Hour.ShouldBe("*"),
            () => expression.Day.ShouldBe("*"),
            () => expression.Month.ShouldBe("*"),
            () => expression.DayOfWeek.ShouldBe(dayOfWeekList)
        );

        // Set hour to an invalid value
        expression.ShouldSatisfyAllConditions
        (
            () => Should.Throw<NotSupportedException>(() => expression.DayOfWeek = "*/1"),
            () => Should.Throw<NotSupportedException>(() => expression.DayOfWeek = "1/2"),
            () => Should.Throw<ArgumentOutOfRangeException>(() => expression.DayOfWeek = $"6-3"),
            () => Should.Throw<ArgumentOutOfRangeException>(() => expression.DayOfWeek = "8"),
            () => Should.Throw<FormatException>(() => expression.DayOfWeek = "a"),
            () => Should.Throw<FormatException>(() => expression.DayOfWeek = "1a"),
            () => Should.Throw<FormatException>(() => expression.DayOfWeek = "1a-2"),
            () => Should.Throw<FormatException>(() => expression.DayOfWeek = "1-2a"),
            () => Should.Throw<FormatException>(() => expression.DayOfWeek = "a1,2"),
            () => Should.Throw<FormatException>(() => expression.DayOfWeek = "1-2a"),
            () => Should.Throw<FormatException>(() => expression.DayOfWeek = "1-2-3")
        );
    }

    [Fact]
    public void ToCronExpressionTest()
    {
        var minute = Random.Shared.Next(0, 60).ToString();
        var hour = Random.Shared.Next(0, 24).ToString();
        var day = Random.Shared.Next(1, 30).ToString();
        var month = Random.Shared.Next(1, 13).ToString();
        var dayOfWeek = Random.Shared.Next(0, 7).ToString();
        var expression = new CronExpression(minute, hour, day, month, dayOfWeek);

        var actual = expression.ToCronExpression();
        var expected = $"{minute} {hour} {day} {month} {dayOfWeek}";

        actual.ShouldBe(expected);
    }

    [Fact]
    public void ToCronExpressionUsingDefaultsTest()
    {
        var expression = new CronExpression();

        var actual = expression.ToCronExpression();
        var expected = "* * * * *";

        actual.ShouldBe(expected);
    }

    [Fact]
    public void ToCronExpressionFailureTest()
    {
        var monthsWith31Days = new[] { 1, 3, 5, 7, 8, 10, 12 };
        var expression = new CronExpression();

        for (var i = 1; i <= 12; i++)
        {
            expression.Month = i.ToString();
            expression.Day = (i == 2) ? "29" : "30";

            _ = expression.ToCronExpression();

            expression.Day = "31";
            if (monthsWith31Days.Contains(i))
            {
                _ = expression.ToCronExpression();
            }
            else
            {
                Should.Throw<ArgumentOutOfRangeException>(() => expression.ToCronExpression());
            }
        }
    }

    [Fact]
    public void ParseTest()
    {
        var minute = Random.Shared.Next(0, 60);
        var hour = Random.Shared.Next(0, 24);
        var day = Random.Shared.Next(1, 30);
        var month = Random.Shared.Next(1, 12);
        var dayOfWeek = Random.Shared.Next(0, 7);

        var value = $"{minute} {hour} {day} {month} {dayOfWeek}";

        var expression = CronExpression.Parse(value);

        expression.ShouldSatisfyAllConditions(
            () => expression.Minute.ShouldBe(minute.ToString()),
            () => expression.Hour.ShouldBe(hour.ToString()),
            () => expression.Day.ShouldBe(day.ToString()),
            () => expression.Month.ShouldBe(month.ToString()),
            () => expression.DayOfWeek.ShouldBe(dayOfWeek.ToString())
        );
    }

    [Theory]
    [InlineData("1 2 3")]
    [InlineData("1 2 3 4 5 6")]
    public void ParseFailureTest(string expression)
    {
        Should.Throw<FormatException>(() => CronExpression.Parse(expression));
    }

    [Fact]
    public void TryParseTest()
    {
        var minute = Random.Shared.Next(0, 60);
        var hour = Random.Shared.Next(0, 24);
        var day = Random.Shared.Next(1, 30);
        var month = Random.Shared.Next(1, 12);
        var dayOfWeek = Random.Shared.Next(0, 7);

        var value = $"{minute} {hour} {day} {month} {dayOfWeek}";

        var result = CronExpression.TryParse(value, out var expression);

        result.ShouldBeTrue();
        expression.ShouldNotBeNull();
        expression.ShouldSatisfyAllConditions(
            () => expression.Minute.ShouldBe(minute.ToString()),
            () => expression.Hour.ShouldBe(hour.ToString()),
            () => expression.Day.ShouldBe(day.ToString()),
            () => expression.Month.ShouldBe(month.ToString()),
            () => expression.DayOfWeek.ShouldBe(dayOfWeek.ToString())
        );
    }

    [Theory]
    [InlineData("1 2 3")]
    [InlineData("1 2 3 4 5 6")]
    public void TryParseFailureTest(string expression)
    {
        var result = CronExpression.TryParse(expression, out var schedule);

        result.ShouldBeFalse();
        schedule.ShouldBeNull();
    }
}
