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

| Field     | Property    | Allowed examples                    | Step syntax | Valid range           |
| --------- | ----------- | ----------------------------------- | ----------- | --------------------- |
| Minute    | `Minute`    | `*`, `0`, `*/5`, `0,15,30`, `10-20` | Yes         | 0–59                  |
| Hour      | `Hour`      | `*`, `0`, `*/4`, `9,17`, `6-18`     | Yes         | 0–23                  |
| Day (DOM) | `Day`       | `*`, `1`, `1,15`, `1-31`, `*/2`     | Yes         | 1–31                  |
| Month     | `Month`     | `*`, `1`, `1,6,12`, `1-12`, `*/3`   | Yes         | 1–12                  |
| DayOfWeek | `DayOfWeek` | `*`, `0`, `1-5`, `0,6`              | **No**      | 0–6 (Sunday–Saturday) |

If a value is outside of these ranges or invalid for its position, the property setter throws a `FormatException`, `ArgumentOutOfRangeException`, or `NotSupportedException`. This ensures that all expressions are valid according to Kubernetes conventions.

A comma‑separated list is validated one element at a time, so each element may itself be a single value, a range, or a step. For example, `5-10,15-31` and `0,30,*/15` are both accepted.

### Unsupported syntax

This library implements a deliberately small grammar. The only characters accepted in any field are `*`, digits, `,`, `-`, and `/`. The following are **not** supported and will throw:

| Not supported          | Examples                        |
| ---------------------- | ------------------------------- |
| Non‑numeric names      | `JAN`, `DEC`, `MON`, `SUN`      |
| Macros / nicknames     | `@daily`, `@hourly`, `@reboot`  |
| Quartz special tokens  | `?`, `L`, `W`, `#`              |
| Range combined with a step | `1-10/2`                    |
| A seconds or year field | `0 * * * * *` (six fields)     |

Expressions always have exactly five fields; there is no seconds mode. A step interval is validated against the range of the field it applies to, so `*/60` in the minute field throws even though `60` looks like a plausible interval.

:::important Invalid Day of Month

The `ToCronExpression()` method also checks that the specified day can actually occur in the specified month. This check runs **only when `Day` is a single numeric value**. If `Day` is `*`, a list, a range, or a step, the check is skipped entirely.

`Month`, on the other hand, may be any valid expression. It is expanded into the full set of months it selects, and the check fails only when the day is invalid for **every** month in that set:

```csharp
new CronExpression("0", "0", "31", "2,4,6").ToCronExpression();
// throws - 31 does not exist in February, April, or June

new CronExpression("0", "0", "31", "1,3").ToCronExpression();
// "0 0 31 1,3 *" - January and March both have 31 days

new CronExpression("0", "0", "31", "*").ToCronExpression();
// "0 0 31 * *" - valid, because some months have 31 days
```

February is treated as having 29 days, since a cron expression carries no year. `29 * * 2 *` is therefore accepted, while day 30 or 31 in February is rejected.

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

`Parse` throws a `FormatException` if the string does not contain exactly 5 parts. Runs of consecutive spaces are tolerated, since empty entries are discarded before the parts are counted.

Each part is then assigned through the corresponding property, so `Parse` also propagates any validation error those setters raise. A string with the right number of parts but a bad value fails with `ArgumentOutOfRangeException` or `NotSupportedException` rather than `FormatException`.

```csharp
// Strict parse - throws FormatException if not exactly 5 parts
var expr = CronExpression.Parse("0 0 * 1 *");

// Right shape, invalid value - throws ArgumentOutOfRangeException
CronExpression.Parse("0 0 * 13 *");

// Non‑throwing parse - returns false for every failure above
if (!CronExpression.TryParse("invalid value", out var result))
{
    // handle error path
}
```

`TryParse` returns `false` for all of these cases and sets `expression` to `null`; it never throws.

## Validation behavior

Setting any field automatically validates that field's syntax and numeric ranges. Validation errors occur immediately at assignment, or when `ToCronExpression()` confirms that a specified day/month combination is valid.

| Scenario                                                | Exception                                                 |
| ------------------------------------------------------- | --------------------------------------------------------- |
| Invalid syntax or non‑numeric token (`a`, `1a`, `1-2-3`) | `FormatException`                                         |
| Value outside the allowed range (`8` for `DayOfWeek`)    | `ArgumentOutOfRangeException`                             |
| Range start not less than end (`6-3`, `5-5`)             | `ArgumentOutOfRangeException`                             |
| Step interval of `0`                                     | `ArgumentOutOfRangeException`                             |
| Step syntax on `DayOfWeek` (`*/1`, `1/2`)                | `NotSupportedException`                                   |
| Empty or malformed `Month` expression (`1/2/3`)          | `ArgumentException` (from `ToCronExpression()`)           |
| Day invalid for every selected month                     | `ArgumentOutOfRangeException` (from `ToCronExpression()`) |

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

* `CronExpression(string? minute = null, string? hour = null, string? day = null, string? month = null, string? dayOfWeek = null)`

There is a single constructor; every parameter is optional, so `new CronExpression()` creates `"* * * * *"`. Any parameter that is omitted or passed as `null` defaults to `"*"`. Arguments are assigned through the properties, so they are validated at construction.

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

* `CronExpression` is `sealed` and cannot be subclassed.
* Properties can be set directly for advanced cases (e.g., mixed lists and ranges).
* Assignments are validated immediately.
* `DayOfWeek` does not support step syntax; assigning `*/2` to it throws `NotSupportedException`.
* `ToString()` is not overridden. Call `ToCronExpression()` to render the expression.
* The type is mutable and not thread‑safe. Treat instances as short‑lived builders or confine them to a single thread.
* Validation is eager and automatic to help surface configuration mistakes early.

## Troubleshooting

* **`FormatException` while setting a property** - The value contains characters or syntax not permitted for that field. Check for names such as `MON`, macros such as `@daily`, or a range combined with a step such as `1-10/2`.
* **`ArgumentOutOfRangeException` while setting a property** - The numeric value is outside the allowed range, or a range's start is not strictly less than its end.
* **`NotSupportedException` while setting `DayOfWeek`** - Step syntax is not available for this field. Use a list (`1,3,5`) or a range (`1-5`) instead.
* **`ArgumentOutOfRangeException` from `ToCronExpression()`** - The day cannot occur in any of the selected months. Adjust the day, or widen the month field to include a month that has that day.
* **`ArgumentException` from `ToCronExpression()`** - The `Month` field is empty or contains a malformed segment such as `1/2/3`.
