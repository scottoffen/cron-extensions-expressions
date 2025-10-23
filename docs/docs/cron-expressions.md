---
sidebar_position: 2
title: CronExpression
---

A small, focused type that represents a Kubernetes‑supported cron expression using five fields: minute, hour, day of month, month, and day of week. It provides built‑in validation and can parse and format cron strings.

## Quick start

```csharp
using Cron.Extensions.Expressions;

// Every day at midnight
var expr = new CronExpression(minute: "0", hour: "0");
var text = expr.ToCronExpression(); // "0 0 * * *"

// Parse an existing expression
var parsed = CronExpression.Parse("*/5 * * * *");

// TryParse pattern
if (CronExpression.TryParse("30 6 * * 1-5", out var weekday630))
{
    Console.WriteLine(weekday630.ToCronExpression()); // "30 6 * * 1-5"
}
```

## Cron format

This library uses the standard 5‑field format that Kubernetes CronJobs expect.

| Field     | Property    | Allowed examples                    | Valid range           |
| --------- | ----------- | ----------------------------------- | --------------------- |
| Minute    | `Minute`    | `*`, `0`, `*/5`, `0,15,30`, `10-20` | 0–59                  |
| Hour      | `Hour`      | `*`, `0`, `*/4`, `9,17`, `6-18`     | 0–23                  |
| Day (DOM) | `Day`       | `*`, `1`, `1,15`, `1-31`            | 1–31                  |
| Month     | `Month`     | `*`, `1`, `1,6,12`, `1-12`          | 1–12                  |
| DayOfWeek | `DayOfWeek` | `*`, `0`, `1-5`, `0,6`              | 0–6 (Sunday–Saturday) |

If a value is outside of these ranges or invalid for its position, the property setter throws a `FormatException` or `ArgumentOutOfRangeException`. This ensures that all expressions are valid according to Kubernetes conventions.

:::important Invalid Day of Month

The `ToCronExpression()` method will also check that the specified day is valid for the given month (for example, February 30 is invalid and will throw an exception), but only if both the day and the month are single values (not a range, list, or increment).

:::

## Creating expressions

You can build an expression incrementally or pass values to the constructor. Omitted fields default to `*`.

```csharp
// Incremental
var hourlyOnTheHalf = new CronExpression
{
    Minute = "30",
    Hour = "*",
};

// With constructor
var weekdaysAt0630 = new CronExpression(minute: "30", hour: "6", dayOfWeek: "1-5");

// Serialize
string text = weekdaysAt0630.ToCronExpression(); // "30 6 * * 1-5"
```

## Parsing

The parser will throw a `FormatException` if the string does not contain exactly 5 parts.

```csharp
// Strict parse - throws FormatException if not exactly 5 parts
var expr = CronExpression.Parse("0 0 * 1 *");

// Non‑throwing parse
if (!CronExpression.TryParse("invalid value", out var result))
{
    // handle error path
}
```

## Validation behavior

Setting any field automatically validates that field's syntax and numeric ranges. Validation errors occur immediately at assignment, or when `ToCronExpression()` confirms that a specified day/month combination is valid.

| Scenario                            | Exception                                                 |
| ----------------------------------- | --------------------------------------------------------- |
| Invalid syntax or token for a field | `FormatException`                                         |
| Value outside the allowed range     | `ArgumentOutOfRangeException`                             |
| Invalid day/month combination       | `ArgumentOutOfRangeException` (from `ToCronExpression()`) |

This ensures that all cron expressions are syntactically and semantically valid before use.

## Common recipes

```csharp
// Every minute
new CronExpression().ToCronExpression();               // "* * * * *"

// Every 5 minutes
new CronExpression(minute: "*/5").ToCronExpression(); // "*/5 * * * *"

// Daily at midnight
new CronExpression(minute: "0", hour: "0").ToCronExpression(); // "0 0 * * *"

// Weekdays at 06:30
new CronExpression("30", "6", dayOfWeek: "1-5").ToCronExpression(); // "30 6 * * 1-5"

// On the 1st and 15th of every month at 09:00
new CronExpression("0", "9", "1,15").ToCronExpression(); // "0 9 1,15 * *"

// Only in January at midnight
new CronExpression("0", "0", month: "1").ToCronExpression(); // "0 0 * 1 *"
```

## API reference

### Constructors

* `CronExpression()`
* `CronExpression(string? minute = null, string? hour = null, string? day = null, string? month = null, string? dayOfWeek = null)`

All omitted parameters default to `"*"`.

### Properties

* `string Minute` - minute component
* `string Hour` - hour component
* `string Day` - day of month component
* `string Month` - month component
* `string DayOfWeek` - day of week component

Each property automatically validates the assigned value.

### Methods

* `string ToCronExpression()` - returns the formatted 5‑field cron string and confirms that an explicit day is valid for the specified month
* `static CronExpression Parse(string value)` - parse a cron string, throws on invalid input
* `static bool TryParse(string value, out CronExpression? expression)` - parse without throwing

## Design notes

* Properties can be set directly for advanced cases (e.g., mixed lists and ranges).
* Assignments are validated immediately.
* `DayOfWeek` does not support increment syntax.
* The type is mutable and not thread‑safe. Treat instances as short‑lived builders or confine them to a single thread.
* Validation is eager and automatic to help surface configuration mistakes early.

## Troubleshooting

* **`FormatException` while setting a property** - The value contains characters or syntax not permitted for that field.
* **`ArgumentOutOfRangeException` while setting a property** - The numeric value is outside the allowed range.
* **`ArgumentOutOfRangeException` from `ToCronExpression()`** - The day is not valid for the specified month. Adjust one of the fields to match a real calendar date.

## Examples in tests

You can mirror the samples above in your unit tests to assert accepted values and expected failures using your preferred test framework.
