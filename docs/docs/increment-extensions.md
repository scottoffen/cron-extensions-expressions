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

| Method                                    | Effect on `CronExpression`                                                                  | Valid range                  |
| ----------------------------------------- | ------------------------------------------------------------------------------------------- | ---------------------------- |
| `EveryMinute()`                           | `Minute = "*"`                                                                              | —                            |
| `EveryXMinutes(int increment)`            | `Minute = "*/{increment}"` (uses `EveryMinute()` when `increment == 1`)                     | 0–59                         |
| `EveryXMinutes(int start, int increment)` | `Minute = "{start}/{increment}"` (uses `EveryMinute()` when `start == 1 && increment == 1`) | start: 0–59, increment: 0–59 |
| `EveryHour()`                             | `Hour = "*"`                                                                                | —                            |
| `EveryXHours(int increment)`              | `Hour = "*/{increment}"` (uses `EveryHour()` when `increment == 1`)                         | 0–23                         |
| `EveryXHours(int start, int increment)`   | `Hour = "{start}/{increment}"` (uses `EveryHour()` when `start == 1 && increment == 1`)     | start: 0–23, increment: 0–23 |
| `EveryDay()`                              | `Day = "*"`                                                                                 | —                            |
| `EveryXDays(int increment)`               | `Day = "*/{increment}"` (uses `EveryDay()` when `increment == 1`)                           | 1–31                         |
| `EveryXDays(int start, int increment)`    | `Day = "{start}/{increment}"` (uses `EveryDay()` when `start == 1 && increment == 1`)       | start: 1–31, increment: 1–31 |
| `EveryMonth()`                            | `Month = "*"`                                                                               | —                            |
| `EveryXMonths(int increment)`             | `Month = "*/{increment}"` (uses `EveryMonth()` when `increment == 1`)                       | 1–12                         |
| `EveryXMonths(int start, int increment)`  | `Month = "{start}/{increment}"` (uses `EveryMonth()` when `start == 1 && increment == 1`)   | start: 1–12, increment: 1–12 |

> There are deliberately no day-of-week increment helpers. Step syntax is not valid in the `DayOfWeek` field, and assigning something like `*/2` to it throws `NotSupportedException`. To restrict the day of week, use [`OnDaysOfWeek`](./list-extensions.md) for a list or [`RangeOfWeek`](./range-extensions.md) for a range. These methods leave `DayOfWeek` untouched.

## Usage examples

### Common intervals

```csharp
// Every minute
new CronExpression().EveryMinute().ToCronExpression(); // "* * * * *"

// Every 5 minutes
new CronExpression().EveryXMinutes(5).ToCronExpression(); // "*/5 * * * *"

// Every 15 minutes starting at :00
new CronExpression().EveryXMinutes(start: 0, increment: 15).ToCronExpression(); // "0/15 * * * *"

// Every hour
new CronExpression().EveryHour().ToCronExpression(); // "* * * * *" with Hour = "*" (minute defaults to "*")

// Every 6 hours
new CronExpression().EveryXHours(6).ToCronExpression(); // "* */6 * * *"

// Every 2 days
new CronExpression().EveryXDays(2).ToCronExpression(); // "* * */2 * *"

// Quarterly (every 3 months)
new CronExpression().EveryXMonths(3).ToCronExpression(); // "* * * */3 *"
```

### Chaining with specific times

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

Assigning out-of-range values results in exceptions from the `CronExpression` properties. For example:

* Minute and hour values must be within 0–59 and 0–23, respectively.
* Day values must be within 1–31; month values within 1–12.
* An `increment` of `0` always throws. For minutes and hours it fails because an interval must be greater than `0`; for days and months it fails the field's 1-based range check first. Either way the exception is `ArgumentOutOfRangeException`.
* An `increment` larger than the field's maximum also throws, so `EveryXMinutes(60)` is rejected.
* `ToCronExpression()` checks that a specified day is valid for the specified month, but only when the day is a single numeric value. A day written as a step, such as the `*/3` produced by `EveryXDays(3)`, skips that check.

> These methods do not perform additional parameter checks beyond setting the corresponding cron field. If you pass an invalid `start` or `increment`, the assignment will fail during property validation.

## Design notes

* Methods return the same `CronExpression` instance for fluent chaining.
* Only the targeted field is modified. Other fields remain whatever they were previously.
* Passing `1` to an `EveryX*` method is optimized to call the corresponding `Every*` method.

