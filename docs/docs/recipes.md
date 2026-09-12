---
sidebar_position: 6
title: Common Recipes
---

This page collects ready-to-use expressions for the schedules people need most often. Each one shows the resulting cron string, so you can copy either the code or the string itself.

## Frequent Intervals

Schedules that repeat many times a day:

```csharp
// Every minute
new CronExpression().ToCronExpression(); // "* * * * *"

// Every 5 minutes
new CronExpression().EveryXMinutes(5).ToCronExpression(); // "*/5 * * * *"

// Every 15 minutes, on the quarter hour
new CronExpression().EveryXMinutes(start: 0, increment: 15).ToCronExpression(); // "0/15 * * * *"

// Every 6 hours
new CronExpression().EveryXHours(6).ToCronExpression(); // "* */6 * * *"
```

## Daily

Once or twice a day, at a fixed time:

```csharp
// Every day at midnight
new CronExpression(minute: "0", hour: "0").ToCronExpression(); // "0 0 * * *"

// Every day at 06:30
new CronExpression(minute: "30", hour: "6").ToCronExpression(); // "30 6 * * *"

// Twice a day, at 09:00 and 17:00
new CronExpression(minute: "0").OnHours(9, 17).ToCronExpression(); // "0 9,17 * * *"
```

## Weekly

Restricted to particular days of the week:

```csharp
// Weekdays at 06:30
new CronExpression("30", "6", dayOfWeek: "1-5").ToCronExpression(); // "30 6 * * 1-5"
// Equivalently, using names directly: dayOfWeek: "MON-FRI" produces the same "1-5"

// Every Monday, Wednesday, and Friday at noon
new CronExpression(minute: "0", hour: "12")
    .OnDaysOfWeek(1, 3, 5)
    .ToCronExpression(); // "0 12 * * 1,3,5"

// Weekends only, at 09:00
new CronExpression(minute: "0", hour: "9")
    .OnDaysOfWeek(0, 6)
    .ToCronExpression(); // "0 9 * * 0,6"
// Equivalently, using names directly: dayOfWeek: "SAT-SUN" produces "6-7", matching the same two days
```

## Monthly and Yearly

Restricted to particular days of the month, or particular months:

```csharp
// The 1st and 15th of every month at 09:00
new CronExpression("0", "9", "1,15").ToCronExpression(); // "0 9 1,15 * *"

// The 1st of every month at midnight
new CronExpression("0", "0", "1").ToCronExpression(); // "0 0 1 * *"

// Quarterly, on the 1st at midnight
new CronExpression(minute: "0", hour: "0", day: "1")
    .OnMonths(1, 4, 7, 10)
    .ToCronExpression(); // "0 0 1 1,4,7,10 *"
// Equivalently, using names directly: month: "JAN,APR,JUL,OCT" produces the same "1,4,7,10"

// Only in January, at midnight
new CronExpression("0", "0", month: "1").ToCronExpression(); // "0 0 * 1 *"
// Equivalently, using the name directly: month: "JAN" produces the same "1"
```

## Macros

The standard crontab macros expand to the equivalent five-field expression through `Parse`, so you can use whichever reads more clearly for a given schedule:

```csharp
CronExpression.Parse("@yearly").ToCronExpression();  // "0 0 1 1 *"  - once a year, midnight on Jan 1
CronExpression.Parse("@monthly").ToCronExpression(); // "0 0 1 * *"  - once a month, midnight on the 1st
CronExpression.Parse("@weekly").ToCronExpression();  // "0 0 * * 0"  - once a week, midnight on Sunday
CronExpression.Parse("@daily").ToCronExpression();   // "0 0 * * *"  - once a day, at midnight
CronExpression.Parse("@hourly").ToCronExpression();  // "0 * * * *"  - once an hour, on the hour
```

See [Macros](./cron-format.md#macros) for the full list, including the `@annually`/`@midnight` aliases and why `@reboot` isn't supported.

## Business Hours

Combining a time-of-day range with a day-of-week range:

```csharp
// Every 15 minutes during business hours, weekdays only
new CronExpression()
    .EveryXMinutes(15)
    .RangeOfHours(8, 17)
    .RangeOfWeek(1, 5)
    .ToCronExpression(); // "*/15 8-17 * * 1-5"

// Hourly on the hour, 09:00 to 17:00, weekdays only
new CronExpression(minute: "0")
    .RangeOfHours(9, 17)
    .RangeOfWeek(1, 5)
    .ToCronExpression(); // "0 9-17 * * 1-5"

// Every other hour during business hours, weekdays only - a range combined with a step
new CronExpression(minute: "0", hour: "9-17/2", dayOfWeek: "1-5").ToCronExpression(); // "0 9-17/2 * * 1-5"
```

:::note[A few of these read differently than they look]

* `EveryXDays(2)` produces `*/2`, which selects the **odd** days (1st, 3rd, 5th …), because a `*/step` counts from the field's minimum and the day field starts at `1`. Use `EveryXDays(2, 2)` for the even days.
* Setting both a day of month and a day of week means **either** matches, not both. `0 9 1 * 1` runs on the 1st *and* on every Monday.
* `29 * * 2 *` is valid but only runs in leap years. Day 30 or 31 in February is rejected outright.

See [Cron Expression Format](./cron-format.md) for the full rules behind each of these.

:::
