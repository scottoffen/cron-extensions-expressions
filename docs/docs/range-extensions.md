---
sidebar_position: 5
title: Range Extensions
---

Fluent helpers for defining contiguous ranges in a `CronExpression`. These methods set specific fields using `[start]-[end]` notation, allowing for easily defined time blocks.

## Overview

`RangeExtensions` provides `RangeOf*` methods for all five cron fields: minutes, hours, days, months, and days of the week. Each method replaces the corresponding field on the `CronExpression` with a start–end range string.

```csharp
using Cron.Extensions.Expressions;

var expr = new CronExpression()
    .RangeOfMinutes(0, 30) // first half of each hour
    .RangeOfHours(8, 17)   // typical work hours
    .RangeOfDays(1, 15)    // first half of the month
    .RangeOfMonths(1, 6)   // January through June
    .RangeOfWeek(1, 5);    // Monday through Friday

var text = expr.ToCronExpression(); // "0-30 8-17 1-15 1-6 1-5"
```

## Method reference

| Method                               | Field modified | Example input | Resulting cron field | Valid range           |
| ------------------------------------ | -------------- | ------------- | -------------------- | --------------------- |
| `RangeOfMinutes(int start, int end)` | Minute         | `0, 30`       | `0-30`               | 0–59                  |
| `RangeOfHours(int start, int end)`   | Hour           | `8, 17`       | `8-17`               | 0–23                  |
| `RangeOfDays(int start, int end)`    | Day of month   | `1, 15`       | `1-15`               | 1–31                  |
| `RangeOfMonths(int start, int end)`  | Month          | `1, 6`        | `1-6`                | 1–12                  |
| `RangeOfWeek(int start, int end)`    | Day of week    | `1, 5`        | `1-5`                | 0–6 (Sunday–Saturday) |

Each method overwrites the target field and returns the same `CronExpression` instance, allowing fluent chaining.

## Usage examples

### Common ranges

```csharp
// Every minute between 0 and 30
new CronExpression().RangeOfMinutes(0, 30).ToCronExpression(); // "0-30 * * * *"

// Every hour between 8 AM and 5 PM
new CronExpression().RangeOfHours(8, 17).ToCronExpression(); // "* 8-17 * * *"

// Every day between the 1st and 15th
new CronExpression().RangeOfDays(1, 15).ToCronExpression(); // "* * 1-15 * *"

// Run between January and June
new CronExpression().RangeOfMonths(1, 6).ToCronExpression(); // "* * * 1-6 *"

// Weekdays only (Monday–Friday)
new CronExpression().RangeOfWeek(1, 5).ToCronExpression(); // "* * * * 1-5"
```

### Combining ranges

You can combine range-based methods with list or increment-based methods to create precise schedules.

```csharp
// Every 15 minutes during business hours, weekdays only
new CronExpression()
    .EveryXMinutes(15)
    .RangeOfHours(8, 17)
    .RangeOfWeek(1, 5)
    .ToCronExpression(); // "*/15 8-17 * * 1-5"
```

## Validation and behavior

* Each range must use values within the allowed field range.
* A range where the start value is greater than or equal to the end value will trigger an exception when the expression is validated.
* Day and month ranges that produce invalid dates (e.g., February 30) will raise an error when `ToCronExpression()` is called.

## Design notes

* Methods return the same `CronExpression` instance for fluent chaining.
* Only the targeted field is modified.
* Ranges follow the standard cron syntax of `start-end`, inclusive on both bounds.
