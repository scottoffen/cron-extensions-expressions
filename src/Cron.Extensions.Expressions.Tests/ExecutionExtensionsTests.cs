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
        // Use a fixed date away from DST boundaries so AddHours(1) in the implementation doesn't cross transitions.
        var baseDate = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Local);

        for (var hour = 0; hour < 24; hour++)
        {
            var expression = new CronExpression
            {
                Hour = hour.ToString()
            };

            // Set now to 30 minutes before the target hour (wrapping so hour=0 uses previous day 23:30).
            var now = baseDate.AddHours((hour + 23) % 24).AddMinutes(30);
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
    public void GetNextExecution_WithUtcStart_ShouldReturnNextRunTimeInUtc()
    {
        // With UTC, there is no DST; arithmetic is unambiguous and correct.
        var baseUtc = new DateTime(2026, 7, 15, 9, 30, 0, DateTimeKind.Utc);
        var expression = new CronExpression { Hour = "10" };

        var actual = expression.GetNextExecution(baseUtc);
        var expected = new DateTime(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc);

        actual.ShouldBe(expected);
        actual.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void GetNextExecution_WhenNextRunFallsInSkippedHour_MayReturnInvalidLocalTime()
    {
        // US Pacific: March 8 2026 2:00 AM does not exist (spring forward to 3:00 AM).
        // The implementation uses DateTime.AddHours, which does not apply DST rules,
        // so it can return 2:00 AM. This test documents that the returned time may be
        // invalid in the given zone (callers can use TimeZoneInfo.IsInvalidTime to check).
        TimeZoneInfo? pacific = null;
        try
        {
            pacific = TimeZoneInfo.FindSystemTimeZoneById(
                OperatingSystem.IsWindows() ? "Pacific Standard Time" : "America/Los_Angeles");
        }
        catch (TimeZoneNotFoundException)
        {
            return; // No Pacific zone on this system; skip test
        }

        // 1:30 AM Pacific on March 8 2026 = 9:30 UTC (PST is -08:00)
        var startUtc = new DateTime(2026, 3, 8, 9, 30, 0, DateTimeKind.Utc);
        var start = TimeZoneInfo.ConvertTimeFromUtc(startUtc, pacific);
        var expression = new CronExpression { Hour = "2" };

        var actual = expression.GetNextExecution(start);

        actual.Year.ShouldBe(2026);
        actual.Month.ShouldBe(3);
        actual.Day.ShouldBe(8);
        actual.Hour.ShouldBe(2);
        actual.Minute.ShouldBe(0);
        // In Pacific, 2:00 AM on this date is invalid (skipped by DST).
        pacific.IsInvalidTime(actual).ShouldBeTrue();
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
        // Day of month is 1-based, so "*/increment" is anchored at day 1: 1, 1+increment, 1+2*increment, ...
        var increment = 5;
        var expression = new CronExpression
        {
            Day = $"*/{increment}"
        };

        var validDays = new List<int>();
        for (var d = 1; d <= 31; d += increment) validDays.Add(d);

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

            var nextDay = validDays.FirstOrDefault(d => d >= now.Day);
            if (nextDay == 0) nextDay = validDays.First();

            var expected = now.AddDays(-now.Day + nextDay);
            if (nextDay < now.Day) expected = expected.AddMonths(1);

            if (validDays.Contains(now.Day)) expected = expected.AddMinutes(1);

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void GetNextExecution_ShouldHandleMonthIncrements()
    {
        // Month is 1-based, so "*/increment" is anchored at month 1: 1, 1+increment, 1+2*increment, ...
        var increment = 2;
        var expression = new CronExpression
        {
            Month = $"*/{increment}"
        };

        var validMonths = new List<int>();
        for (var m = 1; m <= 12; m += increment) validMonths.Add(m);

        var now = GetNow().AddYears(1);
        now = now
            .AddMinutes(-now.Minute)
            .AddHours(-now.Hour)
            .AddDays(-now.Day + 1);

        for (var i = 1; i <= 12; i++)
        {
            now = now.AddMonths(-now.Month + i);
            var actual = expression.GetNextExecution(now);

            var nextMonth = validMonths.FirstOrDefault(m => m >= now.Month);
            if (nextMonth == 0) nextMonth = validMonths.First();

            var expected = now.AddMonths(-now.Month + nextMonth);
            if (nextMonth < now.Month) expected = expected.AddYears(1);

            if (validMonths.Contains(now.Month)) expected = expected.AddMinutes(1);

            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void WillRunOn_ShouldReturnTrue()
    {
        // Day of month is 1-based, so "*/2" is anchored at day 1: 1, 3, 5, ..., 31.
        var expression = new CronExpression(day: "*/2");

        for (var i = 1; i <= 31; i++)
        {
            var expected = i % 2 != 0;

            var date = new DateTime(2024, 10, i);
            var actual = expression.WillRunOn(date);
            actual.ShouldBe(expected);
        }
    }

    [Fact]
    public void WillRunOn_WithMonthStepBelowStart_ShouldNotMatchMonthsBeforeStart()
    {
        // "4/3" should only ever match months >= 4 (April, July, October), never January.
        var expression = new CronExpression(month: "4/3");

        var expectedMonths = new HashSet<int> { 4, 7, 10 };

        for (var month = 1; month <= 12; month++)
        {
            var expected = expectedMonths.Contains(month);
            var date = new DateTime(2024, month, 1);
            var actual = expression.WillRunOn(date);
            actual.ShouldBe(expected, $"month: {month}");
        }
    }

    [Fact]
    public void WillRunOn_WithMonthWildcardStep_ShouldMatchKubernetesCompatibleMonths()
    {
        // "*/5" should be anchored at month 1 (Jan, Jun, Nov), not month 0 (May, Oct).
        var expression = new CronExpression(month: "*/5");

        var expectedMonths = new HashSet<int> { 1, 6, 11 };

        for (var month = 1; month <= 12; month++)
        {
            var expected = expectedMonths.Contains(month);
            var date = new DateTime(2024, month, 1);
            var actual = expression.WillRunOn(date);
            actual.ShouldBe(expected, $"month: {month}");
        }
    }

    [Fact]
    public void WillRunOn_WithRangeAndStep_ShouldMatchOnlyStepsWithinTheRange()
    {
        // "5-10/2" should match 5, 7, and 9 - never a value outside 5-10, even where the step
        // would otherwise land on it (e.g. 11).
        var expression = new CronExpression(minute: "5-10/2");

        var expectedMinutes = new HashSet<int> { 5, 7, 9 };

        for (var minute = 0; minute <= 59; minute++)
        {
            var expected = expectedMinutes.Contains(minute);
            var date = new DateTime(2024, 10, 1, 0, minute, 0);
            var actual = expression.WillRunOn(date);
            actual.ShouldBe(expected, $"minute: {minute}");
        }
    }

    [Fact]
    public void WillRunOn_WithRangeAndStep_UpperBoundIsExclusiveOfValuesPastTheRangeEnd()
    {
        // "8-17/3" should match 8, 11, 14, 17 - the step would next land on 20, which is
        // outside the range and must not match even though 20 is a valid hour.
        var expression = new CronExpression(hour: "8-17/3");

        var expectedHours = new HashSet<int> { 8, 11, 14, 17 };

        for (var hour = 0; hour <= 23; hour++)
        {
            var expected = expectedHours.Contains(hour);
            var date = new DateTime(2024, 10, 1, hour, 0, 0);
            var actual = expression.WillRunOn(date);
            actual.ShouldBe(expected, $"hour: {hour}");
        }
    }

    [Fact]
    public void WillRunOn_BuiltFromNames_MatchesTheSameDatesAsTheNumericEquivalent()
    {
        // Names are translated to numeric form at assignment, so an expression built from
        // names must match exactly the same dates as the equivalent numeric expression - this
        // is the end-to-end confirmation that ExecutionExtensions never needs to know names
        // exist.
        var expression = new CronExpression(month: "JAN-JUN", dayOfWeek: "MON-FRI");

        var expectedMonths = new HashSet<int> { 1, 2, 3, 4, 5, 6 };
        var expectedDaysOfWeek = new HashSet<DayOfWeek>
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
        };

        for (var month = 1; month <= 12; month++)
        {
            for (var day = 1; day <= 7; day++)
            {
                var date = new DateTime(2024, month, day);
                var expected = expectedMonths.Contains(month) && expectedDaysOfWeek.Contains(date.DayOfWeek);
                expression.WillRunOn(date).ShouldBe(expected, $"{date:yyyy-MM-dd} ({date.DayOfWeek})");
            }
        }
    }

    [Fact]
    public void WillRunOn_WithDayOfWeekSeven_MatchesSundayJustLikeZero()
    {
        // Per the cron specification, "7" is an alias for Sunday. CronExpression stores "7"
        // exactly as assigned (it is not normalized to "0"), so this confirms matching itself
        // treats them as equivalent.
        var expressionWithSeven = new CronExpression(dayOfWeek: "7");
        var expressionWithZero = new CronExpression(dayOfWeek: "0");

        for (var day = 1; day <= 31; day++)
        {
            var date = new DateTime(2024, 10, day);
            var expected = date.DayOfWeek == DayOfWeek.Sunday;

            expressionWithSeven.WillRunOn(date).ShouldBe(expected, $"day: {day}");
            expressionWithZero.WillRunOn(date).ShouldBe(expected, $"day: {day}");
        }
    }

    [Fact]
    public void WillRunOn_WithDayOfWeekRangeEndingInSeven_MatchesThroughSunday()
    {
        // "5-7" must mean Friday, Saturday, Sunday - not throw, and not silently drop Sunday.
        var fridayThroughSunday = new CronExpression(dayOfWeek: "5-7");

        for (var day = 1; day <= 31; day++)
        {
            var date = new DateTime(2024, 10, day);
            var dow = date.DayOfWeek;
            var expected = dow == DayOfWeek.Friday || dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday;

            fridayThroughSunday.WillRunOn(date).ShouldBe(expected, $"day: {day}");
        }
    }

    [Fact]
    public void WillRunOn_WithDayOfWeekListIncludingSeven_MatchesSundayAlongsideOtherDays()
    {
        // "1,7" must mean Monday and Sunday.
        var mondayAndSunday = new CronExpression(dayOfWeek: "1,7");

        for (var day = 1; day <= 31; day++)
        {
            var date = new DateTime(2024, 10, day);
            var dow = date.DayOfWeek;
            var expected = dow == DayOfWeek.Monday || dow == DayOfWeek.Sunday;

            mondayAndSunday.WillRunOn(date).ShouldBe(expected, $"day: {day}");
        }
    }

    [Fact]
    public void WillRunOn_WithDayOfWeekRangeNotIncludingSeven_DoesNotSpuriouslyMatchSunday()
    {
        // A range that never touches 7 (e.g. "1-5", Monday-Friday) must not be affected by the
        // 7-for-Sunday handling - Sunday must still correctly fail to match.
        var weekdays = new CronExpression(dayOfWeek: "1-5");

        for (var day = 1; day <= 31; day++)
        {
            var date = new DateTime(2024, 10, day);
            var dow = date.DayOfWeek;
            var expected = dow >= DayOfWeek.Monday && dow <= DayOfWeek.Friday;

            weekdays.WillRunOn(date).ShouldBe(expected, $"day: {day}");
        }
    }

    [Fact]
    public void WillRunOn_WithBothDayAndDayOfWeekRestricted_ShouldMatchEitherField()
    {
        // Standard (Vixie/Kubernetes) cron semantics: when both day-of-month and day-of-week
        // are restricted, a date matches if EITHER matches - e.g. "1,15" and Friday means the
        // 1st, the 15th, or any Friday, not only a 1st/15th that happens to fall on a Friday.
        var expression = new CronExpression(day: "1,15", dayOfWeek: "5");

        for (var day = 1; day <= 31; day++)
        {
            var date = new DateTime(2024, 10, day);
            var expected = day == 1 || day == 15 || date.DayOfWeek == DayOfWeek.Friday;

            var actual = expression.WillRunOn(date);
            actual.ShouldBe(expected, $"day: {day}");
        }
    }

    [Fact]
    public void WillRunOn_WithOnlyDayOfWeekRestricted_ShouldRequireDayOfWeekMatchOnly()
    {
        // The OR rule only applies when BOTH fields are restricted. With day-of-month left
        // as "*", only day-of-week needs to match.
        var expression = new CronExpression(dayOfWeek: "5");

        for (var day = 1; day <= 31; day++)
        {
            var date = new DateTime(2024, 10, day);
            var expected = date.DayOfWeek == DayOfWeek.Friday;

            var actual = expression.WillRunOn(date);
            actual.ShouldBe(expected, $"day: {day}");
        }
    }

    [Fact]
    public void GetNextExecution_WithBothDayAndDayOfWeekRestricted_ShouldMatchEitherField()
    {
        // Standard (Vixie/Kubernetes) cron semantics: when both day-of-month and day-of-week
        // are restricted, a match on EITHER field is sufficient, not only when both match
        // simultaneously.
        var days = new List<int> { 1, 15 };
        var daysOfWeek = new List<int> { 5 }; // Friday

        var expression = new CronExpression
        {
            Day = string.Join(",", days),
            DayOfWeek = string.Join(",", daysOfWeek)
        };

        var now = GetNow();
        now = now
            .AddMinutes(-now.Minute)
            .AddHours(-now.Hour)
            .AddMonths(-now.Month + 7)
            .AddYears(1)
            .AddDays(-now.Day + 1);

        for (var i = 0; i < 40; i++)
        {
            var matchesNow = days.Contains(now.Day) || daysOfWeek.Contains((int)now.DayOfWeek);
            var actual = expression.GetNextExecution(now);

            DateTime expected;
            if (matchesNow)
            {
                expected = now.AddMinutes(1);
            }
            else
            {
                expected = now.AddDays(1);
                while (!(days.Contains(expected.Day) || daysOfWeek.Contains((int)expected.DayOfWeek)))
                {
                    expected = expected.AddDays(1);
                }
            }

            actual.ShouldBe(expected, $"now: {now:yyyy-MM-dd}");
            now = now.AddDays(1);
        }
    }

    /// <summary>
    /// Runs <paramref name="action"/> on a background task and asserts it throws
    /// <typeparamref name="TException"/> within <paramref name="timeoutSeconds"/>.
    /// </summary>
    /// <remarks>
    /// These tests guard against a hang, not just a wrong exception. A blocking wait would
    /// trip xUnit1031 and risk deadlocking the test host, so the timeout is awaited rather
    /// than blocked on.
    /// </remarks>
    private static async Task ShouldThrowWithinTimeout<TException>(Action action, string because, int timeoutSeconds = 5)
        where TException : Exception
    {
        var work = Task.Run(() => Should.Throw<TException>(action));
        var winner = await Task.WhenAny(work, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)));

        (winner == work).ShouldBeTrue(because);

        // Surface any assertion failure raised inside the task.
        await work;
    }

    [Fact]
    public async Task GetNextExecution_WithUnsatisfiableDayMonthCombination_ThrowsInsteadOfHanging()
    {
        // GetNextExecution has no termination guard of its own - an expression that can never
        // match (day 30 can never occur in February) must be rejected before the search even
        // starts, the same way ToCronExpression() already rejects it.
        var expression = new CronExpression(day: "30", month: "2");

        await ShouldThrowWithinTimeout<ArgumentOutOfRangeException>(
            () => expression.GetNextExecution(),
            "GetNextExecution did not return within 5 seconds.");
    }

    [Fact]
    public async Task GetNextExecution_WithUnsatisfiableDayAsListOrRange_ThrowsInsteadOfHanging()
    {
        // A single numeric Day was already caught before this fix. Day expressed as a list or
        // range must be checked the same way - "30,31" and "30-31" are just as unsatisfiable
        // against February as a bare "30" is.
        var listExpression = new CronExpression(day: "30,31", month: "2");
        var rangeExpression = new CronExpression(day: "30-31", month: "2");

        await ShouldThrowWithinTimeout<ArgumentOutOfRangeException>(
            () => listExpression.GetNextExecution(),
            "list case did not return within 5 seconds.");

        await ShouldThrowWithinTimeout<ArgumentOutOfRangeException>(
            () => rangeExpression.GetNextExecution(),
            "range case did not return within 5 seconds.");
    }

    [Fact]
    public void GetNextExecution_UnsatisfiableExpression_ThrowsSameExceptionAsToCronExpression()
    {
        var expression = new CronExpression(day: "30", month: "2");

        var fromToCronExpression = Should.Throw<ArgumentOutOfRangeException>(() => expression.ToCronExpression());
        var fromGetNextExecution = Should.Throw<ArgumentOutOfRangeException>(() => expression.GetNextExecution());

        fromGetNextExecution.ParamName.ShouldBe(fromToCronExpression.ParamName);
        fromGetNextExecution.Message.ShouldBe(fromToCronExpression.Message);
    }

    [Fact]
    public void GetNextExecution_WithSatisfiableButRareDayMonthCombination_StillFindsAMatch()
    {
        // Day 29 combined with February is NOT unsatisfiable - it just requires a leap year.
        // This must not be mistaken for the truly-impossible case (day 30 or 31 in February)
        // and rejected up front.
        var expression = new CronExpression(minute: "0", hour: "0", day: "29", month: "2");

        var next = expression.GetNextExecution(new DateTime(2025, 1, 1));

        next.Month.ShouldBe(2);
        next.Day.ShouldBe(29);
        (next.Year % 4).ShouldBe(0);
    }

    [Fact]
    public void GetNextExecution_WithLeapDayAcrossCenturyBoundary_StillFindsAMatchWithinDefaultHorizon()
    {
        // 1900 was not a leap year, so the gap from 1896 to 1904 is 8 years - the worst-case
        // gap for day 29 combined with February. This must still resolve within the default
        // 10-year search horizon.
        var expression = new CronExpression(minute: "0", hour: "0", day: "29", month: "2");

        var next = expression.GetNextExecution(new DateTime(1896, 3, 1));

        next.Year.ShouldBe(1904);
        next.Month.ShouldBe(2);
        next.Day.ShouldBe(29);
    }

    [Fact]
    public void GetNextExecution_WithMaxSearchYearsLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        var expression = CronExpression.Parse("* * * * *");

        Should.Throw<ArgumentOutOfRangeException>(() => expression.GetNextExecution(maxSearchYears: 0));
        Should.Throw<ArgumentOutOfRangeException>(() => expression.GetNextExecution(maxSearchYears: -1));
    }

    [Fact]
    public void GetNextExecution_WhenSearchExceedsMaxSearchYears_ThrowsCronSearchHorizonExceededException()
    {
        // The next Feb 29 after March 1, 2024 is Feb 29, 2028 - 4 years out, which exceeds a
        // deliberately small 1-year horizon. This is the realistic way to exercise the guard,
        // since a truly unsatisfiable expression is now rejected before the search even starts.
        var expression = new CronExpression(minute: "0", hour: "0", day: "29", month: "2");
        var start = new DateTime(2024, 3, 1);

        var ex = Should.Throw<CronSearchHorizonExceededException>(() => expression.GetNextExecution(start, maxSearchYears: 1));

        ex.ShouldBeAssignableTo<InvalidOperationException>();
        ex.Start.ShouldBe(start);
        ex.MaxSearchYears.ShouldBe(1);
        ex.Expression.ShouldBeSameAs(expression);
    }
}
