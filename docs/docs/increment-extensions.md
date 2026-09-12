---
sidebar_position: 3
title: Increment Extensions
---

Fluent helpers for setting step-based schedules on a `CronExpression`. These extensions mutate the passed instance and return it, so you can chain calls when building schedules.

## Overview

`IncrementExtensions` provides `Every*` and `EveryX*` methods to express intervals for minutes, hours, days, and months. Each method updates a single field on the underlying `CronExpression` and leaves the other fields unchanged.

```csharp
using Cron.Extensions.Expressions;

var expr = new CronExpression()
    .EveryXMinutes(15)  // */15 in the minute field
    .EveryHour()        // * in the hour field
    .EveryDay()         // * in the day-of-month field
    .EveryMonth();      // * in the month field

var text = expr.ToCronExpression(); // "*/15 * * * *"
```

## Method reference

Every method sets a single field to a step interval, either a fixed increment or a starting point plus increment:

| Method                                    | Effect on `CronExpression`                                                                  | Valid values                 |
| ----------------------------------------- | ------------------------------------------------------------------------------------------- | ---------------------------- |
| `EveryMinute()`                           | `Minute = "*"`                                                                              | —                            |
| `EveryXMinutes(int increment)`            | `Minute = "*/{increment}"` (uses `EveryMinute()` when `increment == 1`)                     | increment: 1–59              |
| `EveryXMinutes(int start, int increment)` | `Minute = "{start}/{increment}"` (uses `EveryMinute()` when `start == 1 && increment == 1`) | start: 0–59, increment: 1–59 |
| `EveryHour()`                             | `Hour = "*"`                                                                                | —                            |
| `EveryXHours(int increment)`              | `Hour = "*/{increment}"` (uses `EveryHour()` when `increment == 1`)                         | increment: 1–23              |
| `EveryXHours(int start, int increment)`   | `Hour = "{start}/{increment}"` (uses `EveryHour()` when `start == 1 && increment == 1`)     | start: 0–23, increment: 1–23 |
| `EveryDay()`                              | `Day = "*"`                                                                                 | —                            |
| `EveryXDays(int increment)`               | `Day = "*/{increment}"` (uses `EveryDay()` when `increment == 1`)                           | increment: 1–31              |
| `EveryXDays(int start, int increment)`    | `Day = "{start}/{increment}"` (uses `EveryDay()` when `start == 1 && increment == 1`)       | start: 1–31, increment: 1–31 |
| `EveryMonth()`                            | `Month = "*"`                                                                               | —                            |
| `EveryXMonths(int increment)`             | `Month = "*/{increment}"` (uses `EveryMonth()` when `increment == 1`)                       | increment: 1–12              |
| `EveryXMonths(int start, int increment)`  | `Month = "{start}/{increment}"` (uses `EveryMonth()` when `start == 1 && increment == 1`)   | start: 1–12, increment: 1–12 |

:::tip[No day-of-week increment helpers]

There are deliberately no day-of-week increment helpers. Step syntax is not valid in the `DayOfWeek` field, and assigning something like `*/2` to it throws `NotSupportedException`. To restrict the day of week, use [`OnDaysOfWeek`](./list-extensions.md) for a list or [`RangeOfWeek`](./range-extensions.md) for a range. These methods leave `DayOfWeek` untouched.

:::

## Usage examples

### Common intervals

A few of the most common step schedules:

```csharp
// Every minute
new CronExpression().EveryMinute().ToCronExpression(); // "* * * * *"

// Every 5 minutes
new CronExpression().EveryXMinutes(5).ToCronExpression(); // "*/5 * * * *"

// Every 15 minutes starting at :00
new CronExpression().EveryXMinutes(start: 0, increment: 15).ToCronExpression(); // "0/15 * * * *"

// Every hour
new CronExpression().EveryHour().ToCronExpression(); // "* * * * *"

// Every 6 hours
new CronExpression().EveryXHours(6).ToCronExpression(); // "* */6 * * *"

// Every 2 days
new CronExpression().EveryXDays(2).ToCronExpression(); // "* * */2 * *"

// Quarterly (every 3 months)
new CronExpression().EveryXMonths(3).ToCronExpression(); // "* * * */3 *"
```

### Chaining with specific times

Combine an increment with a specific minute and hour by setting those properties directly:

```csharp
// 08:00 every 3 days
var every3DaysAt8 = new CronExpression()
    .EveryXDays(3)
    .EveryMonth();  // keep all months

every3DaysAt8.Minute = "0";
every3DaysAt8.Hour = "8";

var text = every3DaysAt8.ToCronExpression(); // "0 8 */3 * *"
```

## Validation and errors

Assigning out-of-range values results in exceptions from the `CronExpression` properties.

| Scenario                                                                | Exception                     | Notes                                                                                                                                              |
| ------------------------------------------------------------------------- | ------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| `start` or `increment` outside the field's range (e.g. hour `24`, day `32`) | `ArgumentOutOfRangeException` | Same ranges as [`CronExpression`](./cron-expressions.md): minute/hour are 0–59/0–23; day/month are 1–31/1–12.                                     |
| `increment` of `0`                                                         | `ArgumentOutOfRangeException` | Reported as an interval error (`Interval value 0 for <field> must be greater than 0`) rather than a field-range error.                          |
| `increment` larger than the field's maximum                               | `ArgumentOutOfRangeException` | `EveryXMinutes(60)` is rejected this way.                                                                                                           |

These methods perform no additional parameter checks beyond setting the corresponding cron field - an invalid `start` or `increment` fails during property validation, not before.

:::note[A step disables the day/month calendar check]

`ToCronExpression()` checks that a specified day is valid for the specified month, but only when `Day` is a single numeric value. A day written as a step, such as the `*/3` produced by `EveryXDays(3)`, skips that check entirely.

:::

:::note[A `*/n` step counts from the field's minimum]

`EveryXDays(2)` produces `*/2`, which selects the 1st, 3rd, 5th and so on through the 31st, **not** the even-numbered days. Likewise `EveryXMonths(2)` selects January, March, May, and so on. Minutes and hours have a minimum of `0`, so `EveryXMinutes(15)` selects 0, 15, 30, and 45 as you would expect. Use the two-argument overload when you want to pick the starting value yourself, as in `EveryXDays(2, 2)` for the even days.

:::
