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

| Method                                  | Field modified | Example input   | Resulting cron field | Valid range           |
| --------------------------------------- | -------------- | --------------- | -------------------- | --------------------- |
| `OnMinutes(params int[] minutes)`       | Minute         | `0, 15, 30, 45` | `0,15,30,45`         | 0–59                  |
| `OnHours(params int[] hours)`           | Hour           | `6, 12, 18`     | `6,12,18`            | 0–23                  |
| `OnDays(params int[] days)`             | Day of month   | `1, 15, 31`     | `1,15,31`            | 1–31                  |
| `OnMonths(params int[] months)`         | Month          | `1, 6, 12`      | `1,6,12`             | 1–12                  |
| `OnDaysOfWeek(params int[] daysOfWeek)` | Day of week    | `1, 3, 5`       | `1,3,5`              | 0–6 (Sunday–Saturday) |

Each call overwrites the corresponding field of the current `CronExpression`. The provided values are automatically sorted and deduplicated.

## Usage examples

### Basic examples

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

* Each method replaces the existing field with a comma-separated list of numeric values.
* Invalid or out-of-range values cause an exception when the corresponding field on `CronExpression` is set.
* Duplicate values are automatically removed.
* The order of values in the final cron expression is always ascending.

> Example: `OnHours(12, 6, 6)` results in `Hour = "6,12"`.

## Design notes

* Designed for concise, readable syntax when specifying exact times.
* Returns the same `CronExpression` instance for fluent chaining.
* Does not alter unspecified fields.
