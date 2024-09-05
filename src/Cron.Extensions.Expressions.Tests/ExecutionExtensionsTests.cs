namespace Cron.Extensions.Expressions.Tests;

public class ExecutionExtensionsTests
{
    private static DateTime GetNow()
    {
        var now = DateTime.Now;
        return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Kind);
    }

    [Fact]
    public void GetNextExecution_ShouldReturnNextRunTimeAfterNow()
    {
        var expression = new CronExpression();
        var now = GetNow();

        var actual = expression.GetNextExecution();
        var expected = now
            .AddMinutes(1);

        actual.ShouldBe(expected);
    }

    [Fact]
    public void GetNextExecution_ShouldReturnNextRunTimeAfterDate()
    {
        var expression = new CronExpression();
        var now = GetNow().AddHours(1);

        var actual = expression.GetNextExecution(now);
        var expected = now
            .AddMinutes(1);

        actual.ShouldBe(expected);
    }

    [Fact]
    public void GetNextExecution_ShouldReturnNextRunTimeAfterDateWithMinute()
    {
        for (var minute = 0; minute < 60; minute++)
        {
            var expression = new CronExpression
            {
                Minute = minute.ToString()
            };

            var now = GetNow();
            while (now.Minute == minute) now = now.AddMinutes(1);

            var actual = expression.GetNextExecution(now);
            var expected = now
                .AddMinutes(-now.Minute + minute);


            if (now.Minute > minute) expected = expected.AddHours(1);

            actual.ShouldBe(expected);
            actual.Minute.ShouldBe(minute);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldReturnNextRunTimeAfterDateWithHour()
    {
        for (var hour = 0; hour < 24; hour++)
        {
            var expression = new CronExpression
            {
                Hour = hour.ToString()
            };

            var now = GetNow();
            var actual = expression.GetNextExecution(now);

            var expected = (now.Hour == hour)
                ? now.AddMinutes(1)
                : now
                    .AddMinutes(-now.Minute)
                    .AddHours(-now.Hour + hour);

            if (now.Hour > hour) expected = expected.AddDays(1);

            actual.ShouldBe(expected);
            actual.Hour.ShouldBe(hour);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldReturnNextRunTimeAfterDateWithDay()
    {
        for (var day = 1; day <= 28; day++)
        {
            var expression = new CronExpression
            {
                Day = day.ToString()
            };

            var now = GetNow();

            var actual = expression.GetNextExecution(now);
            var expected = (now.Day == day)
                ? now.AddMinutes(1)
                : now
                    .AddMinutes(-now.Minute)
                    .AddHours(-now.Hour)
                    .AddDays(-now.Day + day);

            if (now.Day > day) expected = expected.AddMonths(1);

            actual.ShouldBe(expected);
            actual.Day.ShouldBe(day);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldReturnNextRunTimeAfterDateWithMonth()
    {
        for (var month = 1; month <= 12; month++)
        {
            var expression = new CronExpression
            {
                Month = month.ToString()
            };

            var now = GetNow();

            var actual = expression.GetNextExecution(now);
            var expected = (now.Month == month)
                ? now.AddMinutes(1)
                : now
                    .AddMinutes(-now.Minute)
                    .AddHours(-now.Hour)
                    .AddDays(-now.Day + 1)
                    .AddMonths(-now.Month + month);

            if (now.Month > month) expected = expected.AddYears(1);

            actual.ShouldBe(expected);
            actual.Month.ShouldBe(month);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldReturnNextRunTimeAfterDateWithDayOfWeek()
    {
        for (var dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
        {
            var expression = new CronExpression
            {
                DayOfWeek = dayOfWeek.ToString()
            };

            var now = GetNow();

            var actual = expression.GetNextExecution(now);
            var expected = ((int)now.DayOfWeek == dayOfWeek)
                ? now.AddMinutes(1)
                : now
                    .AddMinutes(-now.Minute)
                    .AddHours(-now.Hour);

            while ((int)expected.DayOfWeek != dayOfWeek) expected = expected.AddDays(1);

            actual.ShouldBe(expected);
            actual.DayOfWeek.ShouldBe((DayOfWeek)dayOfWeek);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleMinuteRanges()
    {
        var start = Random.Shared.Next(5, 15);
        var end = Random.Shared.Next(33, 50);

        var expression = new CronExpression
        {
            Minute = $"{start}-{end}"
        };

        var now = GetNow().AddHours(1);

        for (var i = 0; i < 60; i++)
        {
            now = now.AddMinutes(-now.Minute + i);
            var actual = expression.GetNextExecution(now);

            var expected = (now.Minute < start || now.Minute >= end)
                ? now.AddMinutes(-now.Minute + start)
                : now.AddMinutes(1);

            if (now.Minute >= end) expected = expected.AddHours(1);

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleHourRanges()
    {
        var start = Random.Shared.Next(3, 12);
        var end = Random.Shared.Next(13, 21);

        var expression = new CronExpression
        {
            Hour = $"{start}-{end}"
        };

        var now = GetNow().AddDays(1);
        now = now.AddMinutes(-now.Minute);

        for (var i = 0; i < 24; i++)
        {
            now = now.AddHours(-now.Hour + i);
            var actual = expression.GetNextExecution(now);

            var expected = (now.Hour < start || now.Hour > end)
                ? now.AddHours(-now.Hour + start)
                : now.AddMinutes(1);

            if (now.Hour > end) expected = expected.AddDays(1);

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleDayRanges()
    {
        var start = Random.Shared.Next(5, 15);
        var end = Random.Shared.Next(20, 25);

        var expression = new CronExpression
        {
            Day = $"{start}-{end}"
        };

        var now = GetNow().AddMonths(1);
        now = now
            .AddMinutes(-now.Minute)
            .AddHours(-now.Hour)
            .AddMonths(-now.Month + 7)
            .AddYears(1);

        for (var i = 1; i <= 31; i++)
        {
            now = now.AddDays(-now.Day + i);
            var actual = expression.GetNextExecution(now);

            var expected = (now.Day < start || now.Day > end)
                ? now.AddDays(-now.Day + start)
                : now.AddMinutes(1);

            if (now.Day > end) expected = expected.AddMonths(1);

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleMonthRanges()
    {
        var start = Random.Shared.Next(3, 6);
        var end = Random.Shared.Next(7, 10);

        var expression = new CronExpression
        {
            Month = $"{start}-{end}"
        };

        var now = GetNow().AddYears(1);
        now = now
            .AddMinutes(-now.Minute)
            .AddHours(-now.Hour)
            .AddDays(-now.Day + 1);

        for (var i = 1; i <= 12; i++)
        {
            now = now.AddMonths(-now.Month + i);
            var actual = expression.GetNextExecution(now);

            var expected = (now.Month < start || now.Month > end)
                ? now.AddMonths(-now.Month + start)
                : now.AddMinutes(1);

            if (now.Month > end) expected = expected.AddYears(1);

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleDayOfWeekRanges()
    {
        var start = Random.Shared.Next(1, 3);
        var end = Random.Shared.Next(4, 6);

        var expression = new CronExpression
        {
            DayOfWeek = $"{start}-{end}"
        };

        var now = GetNow();
        now = now
            .AddMinutes(-now.Minute)
            .AddHours(-now.Hour);

        while ((int)now.DayOfWeek != 0) now = now.AddDays(1);

        for (var i = 0; i < 7; i++)
        {
            var actual = expression.GetNextExecution(now);
            var expected = now;

            if (i >= start && i <= end)
            {
                expected = expected.AddMinutes(1);
            }
            else
            {
                while ((int)expected.DayOfWeek != start) expected = expected.AddDays(1);
            }

            actual.ShouldBe(expected);
            now = now.AddDays(1);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleMinuteLists()
    {
        var minutes = new List<int> { 5, 15, 25, 35, 45, 55 };
        var expression = new CronExpression
        {
            Minute = string.Join(",", minutes)
        };

        var now = GetNow().AddHours(1);

        for (var i = 0; i < 60; i++)
        {
            now = now.AddMinutes(-now.Minute + i);
            var actual = expression.GetNextExecution(now);

            var nextMinute = minutes.FirstOrDefault(m => m > now.Minute);
            if (nextMinute == 0) nextMinute = minutes.First();

            var expected = now.AddMinutes(-now.Minute + nextMinute);
            if (nextMinute <= now.Minute) expected = expected.AddHours(1);

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleHourLists()
    {
        var hours = new List<int> { 3, 6, 9, 12, 15, 18, 21 };
        var expression = new CronExpression
        {
            Hour = string.Join(",", hours)
        };

        var now = GetNow().AddDays(1);
        now = now.AddMinutes(-now.Minute);

        for (var i = 0; i < 24; i++)
        {
            now = now.AddHours(-now.Hour + i);
            var actual = expression.GetNextExecution(now);

            var nextHour = hours.FirstOrDefault(h => h >= now.Hour);
            if (nextHour == 0) nextHour = hours.First();

            var expected = now.AddHours(-now.Hour + nextHour);
            if (nextHour < now.Hour) expected = expected.AddDays(1);

            if (hours.Contains(now.Hour)) expected = expected.AddMinutes(1);

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleDayLists()
    {
        var days = new List<int> { 5, 10, 15, 20, 25, 30 };
        var expression = new CronExpression
        {
            Day = string.Join(",", days)
        };

        var now = GetNow();
        now = now
            .AddMinutes(-now.Minute)
            .AddHours(-now.Hour)
            .AddMonths(-now.Month + 7)
            .AddYears(1);

        for (var i = 1; i <= 31; i++)
        {
            now = now.AddDays(-now.Day + i);
            var actual = expression.GetNextExecution(now);

            var nextDay = days.FirstOrDefault(d => d >= now.Day);
            if (nextDay == 0) nextDay = days.First();

            var expected = now.AddDays(-now.Day + nextDay);
            if (nextDay < now.Day) expected = expected.AddMonths(1);

            if (days.Contains(now.Day)) expected = expected.AddMinutes(1);

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleMonthLists()
    {
        var months = new List<int> { 3, 6, 9, 12 };
        var expression = new CronExpression
        {
            Month = string.Join(",", months)
        };

        var now = GetNow().AddYears(1);
        now = now
            .AddMinutes(-now.Minute)
            .AddHours(-now.Hour)
            .AddDays(-now.Day + 1);

        for (var i = 1; i <= 12; i++)
        {
            now = now.AddMonths(-now.Month + i);
            var actual = expression.GetNextExecution(now);

            var nextMonth = months.FirstOrDefault(m => m >= now.Month);
            if (nextMonth == 0) nextMonth = months.First();

            var expected = now.AddMonths(-now.Month + nextMonth);
            if (nextMonth < now.Month) expected = expected.AddYears(1);

            if (months.Contains(now.Month)) expected = expected.AddMinutes(1);

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleDayOfWeekLists()
    {
        var daysOfWeek = new List<int> { 1, 3, 5 };
        var expression = new CronExpression
        {
            DayOfWeek = string.Join(",", daysOfWeek)
        };

        var now = GetNow();
        now = now
            .AddMinutes(-now.Minute)
            .AddHours(-now.Hour);

        while ((int)now.DayOfWeek != 0) now = now.AddDays(1);

        for (var i = 0; i < 7; i++)
        {
            var actual = expression.GetNextExecution(now);
            var expected = now;

            var nextDayOfWeek = daysOfWeek.FirstOrDefault(d => d >= (int)now.DayOfWeek);
            if (nextDayOfWeek == 0) nextDayOfWeek = daysOfWeek.First();

            if (daysOfWeek.Contains((int)now.DayOfWeek))
            {
                expected = expected.AddMinutes(1);
            }
            else
            {
                while ((int)expected.DayOfWeek != nextDayOfWeek) expected = expected.AddDays(1);
            }

            actual.ShouldBe(expected);
            now = now.AddDays(1);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleMinuteIncrements()
    {
        var increment = 4;
        var expression = new CronExpression
        {
            Minute = $"*/{increment}"
        };

        var now = GetNow().AddHours(1);

        for (var i = 0; i < 60; i++)
        {
            now = now.AddMinutes(-now.Minute + i);
            var actual = expression.GetNextExecution(now);

            var nextMinute = (now.Minute / increment + 1) * increment;
            if (nextMinute == 60) nextMinute = 0;
            if (nextMinute == now.Minute) nextMinute += increment;

            var expected = now.AddMinutes(-now.Minute + nextMinute);
            if (nextMinute <= now.Minute) expected = expected.AddHours(1);

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleHourIncrements()
    {
        var increment = 3;
        var expression = new CronExpression
        {
            Hour = $"*/{increment}"
        };

        var now = GetNow().AddDays(1);
        now = now.AddMinutes(-now.Minute);

        for (var i = 0; i < 24; i++)
        {
            now = now.AddHours(-now.Hour + i);
            var actual = expression.GetNextExecution(now);

            var expected = now;
            if (now.Hour % increment == 0)
            {
                expected = expected.AddMinutes(1);
            }
            else
            {
                var nextHour = (now.Hour / increment + 1) * increment;
                if (nextHour == 24) nextHour = 0;

                expected = expected.AddHours(-now.Hour + nextHour);
                if (nextHour < now.Hour) expected = expected.AddDays(1);
            }

            actual.ShouldBe(expected, $"i: {i}");
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleDayIncrements()
    {
        var increment = 5;
        var expression = new CronExpression
        {
            Day = $"*/{increment}"
        };

        var now = GetNow();
        now = now
            .AddMinutes(-now.Minute)
            .AddHours(-now.Hour)
            .AddMonths(-now.Month + 7)
            .AddYears(1);

        for (var i = 1; i <= 31; i++)
        {
            now = now.AddDays(-now.Day + i);
            var actual = expression.GetNextExecution(now);

            var expected = now;
            if (now.Day % increment == 0)
            {
                expected = expected.AddMinutes(1);
            }
            else
            {
                var nextDay = (now.Day / increment + 1) * increment;
                expected = (nextDay >= 31)
                    ? expected.AddDays(-now.Day + increment).AddMonths(1)
                    : expected.AddDays(-now.Day + nextDay);
            }

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleMonthIncrements()
    {
        var increment = 2;
        var expression = new CronExpression
        {
            Month = $"*/{increment}"
        };

        var now = GetNow().AddYears(1);
        now = now
            .AddMinutes(-now.Minute)
            .AddHours(-now.Hour)
            .AddDays(-now.Day + 1);

        for (var i = 1; i <= 12; i++)
        {
            now = now.AddMonths(-now.Month + i);
            var actual = expression.GetNextExecution(now);

            var expected = now;
            if (now.Month % increment == 0)
            {
                expected = expected.AddMinutes(1);
            }
            else
            {
                var nextMonth = (now.Month / increment + 1) * increment;
                expected = (nextMonth > 12)
                    ? expected.AddMonths(-now.Month + increment).AddYears(1)
                    : expected.AddMonths(-now.Month + nextMonth);
            }

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void WillRunOn_ShouldReturnTrue()
    {
        var expression = new CronExpression(day: "*/2");

        for (var i = 1; i <= 31; i++)
        {
            var expected = i % 2 == 0;

            var date = new DateTime(2024, 10, i);
            var actual = expression.WillRunOn(date);
            actual.ShouldBe(expected);
        }

    }
}
