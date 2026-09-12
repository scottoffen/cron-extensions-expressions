---
sidebar_position: 4
title: List Extensions
---

Fluent helpers for setting multiple fixed values for each field of a `CronExpression`. These methods simplify defining schedules that trigger at specific times instead of at regular intervals.

## Overview

`ListExtensions` provides a set of `On*` methods that accept lists of integers corresponding to minutes, hours, days, months, or days of the week. Each method sorts and de-duplicates the provided values, then applies them to the corresponding field of the `CronExpression`.

```csharp
using Cron.Extensions.Expressions;

var expr = new CronExpression()
    .OnMinutes(0, 30)  // top and bottom of the hour
    .OnHours(9, 17)    // 9 AM and 5 PM
    .OnDays(1, 15)     // first and fifteenth day of the month
    .OnMonths(1, 7)    // January and July
    .OnDaysOfWeek(1, 3, 5); // Monday, Wednesday, Friday

var text = expr.ToCronExpression(); // "0,30 9,17 1,15 1,7 1,3,5"
```

## Method reference

Every method replaces one field with a sorted, deduplicated list:

| Method                                  | Field modified | Example input   | Resulting cron field | Valid range           |
| --------------------------------------- | -------------- | --------------- | -------------------- | --------------------- |
| `OnMinutes(params int[] minutes)`       | Minute         | `0, 15, 30, 45` | `0,15,30,45`         | 0–59                  |
| `OnHours(params int[] hours)`           | Hour           | `6, 12, 18`     | `6,12,18`            | 0–23                  |
| `OnDays(params int[] days)`             | Day of month   | `1, 15, 31`     | `1,15,31`            | 1–31                  |
| `OnMonths(params int[] months)`         | Month          | `1, 6, 12`      | `1,6,12`             | 1–12                  |
| `OnDaysOfWeek(params int[] daysOfWeek)` | Day of week    | `1, 3, 5`       | `1,3,5`              | 0–7 (Sunday–Saturday) |

## Usage examples

### Basic examples

A few common fixed-time schedules:

```csharp
// Run at the top and bottom of every hour
new CronExpression().OnMinutes(0, 30).ToCronExpression(); // "0,30 * * * *"

// Run twice daily at 9 AM and 5 PM
new CronExpression().OnHours(9, 17).ToCronExpression(); // "* 9,17 * * *"

// Run on the 1st and 15th of each month
new CronExpression().OnDays(1, 15).ToCronExpression(); // "* * 1,15 * *"

// Run quarterly (Jan, Apr, Jul, Oct)
new CronExpression().OnMonths(1, 4, 7, 10).ToCronExpression(); // "* * * 1,4,7,10 *"

// Run every Monday, Wednesday, and Friday at noon
new CronExpression()
    .OnHours(12)
    .OnDaysOfWeek(1, 3, 5)
    .ToCronExpression(); // "* 12 * * 1,3,5"
```

### Combining with increment helpers

You can chain these with methods from [`IncrementExtensions`](./increment-extensions.md) to build more specific schedules.

```csharp
// Every 15 minutes on weekdays (Monday–Friday)
new CronExpression()
    .EveryXMinutes(15)
    .OnDaysOfWeek(1, 2, 3, 4, 5)
    .ToCronExpression(); // "*/15 * * * 1,2,3,4,5"
```

## Validation and behavior

Each method replaces the existing field with a comma-separated list of numeric values, sorted ascending with duplicates removed - e.g. `OnHours(12, 6, 6)` results in `Hour = "6,12"`.

| Scenario                                                       | Exception                     | Notes                                                                                            |
| ------------------------------------------------------------------ | ------------------------------ | ---------------------------------------------------------------------------------------------------- |
| A value outside the field's allowed range (e.g. `OnHours(24)`) | `ArgumentOutOfRangeException` | Same range as the corresponding [`CronExpression`](./cron-expressions.md) property.                |
| Calling a method with no values (e.g. `OnHours()`)              | `FormatException`             | Produces an empty field, which the property setter rejects. Guard against passing an empty array. |

:::note[A single value re-enables the day/month calendar check]

A single value is written without separators, so `OnDays(15)` sets `Day = "15"`. Because that is a single numeric day, it re-enables the day/month calendar check performed by `ToCronExpression()`.

:::

:::note[OnDaysOfWeek and the 7-for-Sunday alias]

`OnDaysOfWeek` treats `0` and `7` as distinct values, since neither is rewritten to the other - `OnDaysOfWeek(0, 7)` keeps both and sorts to `"0,7"` rather than collapsing to a single `0`. See [`CronExpression`](./cron-expressions.md) for how the `0`/`7` equivalence for Sunday works.

:::
