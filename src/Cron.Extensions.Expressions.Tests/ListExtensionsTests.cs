namespace Cron.Extensions.Expressions.Tests;

public class ListExtensionsTests
{
    [Fact]
    public void OnMinutesTest()
    {
        var minute = Random.Shared.Next(0, 60);
        var minutes = new List<int> { 0, 15, 30, 45 };
        var expression = new CronExpression();

        expression.OnMinutes(minute);
        expression.Minute.ShouldBe(minute.ToString());

        expression.OnMinutes([.. minutes]);
        expression.Minute.ShouldBe("0,15,30,45");

        minutes.Add(15);

        expression.OnMinutes([.. minutes]);
        expression.Minute.ShouldBe("0,15,30,45");

        minutes.Reverse();
        expression.OnMinutes([.. minutes]);
        expression.Minute.ShouldBe("0,15,30,45");
    }

    [Fact]
    public void OnHoursTest()
    {
        var hour = Random.Shared.Next(0, 24);
        var hours = new List<int> { 0, 6, 12, 18 };
        var expression = new CronExpression();

        expression.OnHours(hour);
        expression.Hour.ShouldBe(hour.ToString());

        expression.OnHours([.. hours]);
        expression.Hour.ShouldBe("0,6,12,18");

        hours.Add(6);

        expression.OnHours([.. hours]);
        expression.Hour.ShouldBe("0,6,12,18");

        hours.Reverse();
        expression.OnHours([.. hours]);
        expression.Hour.ShouldBe("0,6,12,18");
    }

    [Fact]
    public void OnDaysTest()
    {
        var day = Random.Shared.Next(1, 31);
        var days = new List<int> { 1, 8, 15, 22 };
        var expression = new CronExpression();

        expression.OnDays(day);
        expression.Day.ShouldBe(day.ToString());

        expression.OnDays([.. days]);
        expression.Day.ShouldBe("1,8,15,22");

        days.Add(8);

        expression.OnDays([.. days]);
        expression.Day.ShouldBe("1,8,15,22");

        days.Reverse();
        expression.OnDays([.. days]);
        expression.Day.ShouldBe("1,8,15,22");
    }

    [Fact]
    public void OnMonthsTest()
    {
        var month = Random.Shared.Next(1, 13);
        var months = new List<int> { 1, 4, 7, 10 };
        var expression = new CronExpression();

        expression.OnMonths(month);
        expression.Month.ShouldBe(month.ToString());

        expression.OnMonths([.. months]);
        expression.Month.ShouldBe("1,4,7,10");

        months.Add(4);

        expression.OnMonths([.. months]);
        expression.Month.ShouldBe("1,4,7,10");

        months.Reverse();
        expression.OnMonths([.. months]);
        expression.Month.ShouldBe("1,4,7,10");
    }

    [Fact]
    public void OnDaysOfWeekTest()
    {
        var dayOfWeek = Random.Shared.Next(0, 7);
        var daysOfWeek = new List<int> { 0, 2, 4, 6 };
        var expression = new CronExpression();

        expression.OnDaysOfWeek(dayOfWeek);
        expression.DayOfWeek.ShouldBe(dayOfWeek.ToString());

        expression.OnDaysOfWeek([.. daysOfWeek]);
        expression.DayOfWeek.ShouldBe("0,2,4,6");

        daysOfWeek.Add(2);

        expression.OnDaysOfWeek([.. daysOfWeek]);
        expression.DayOfWeek.ShouldBe("0,2,4,6");

        daysOfWeek.Reverse();
        expression.OnDaysOfWeek([.. daysOfWeek]);
        expression.DayOfWeek.ShouldBe("0,2,4,6");
    }
}
