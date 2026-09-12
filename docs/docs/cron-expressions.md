---
sidebar_position: 2
title: CronExpression
---

A small, focused type that represents a Kubernetes‑supported cron expression using five fields: minute, hour, day of month, month, and day of week. It provides built‑in validation and can parse and format cron strings.

## Quick start

The constructor, `ToCronExpression()`, and `Parse`/`TryParse` cover the most common cases:

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

## API reference

### Constructors

There is a single constructor; every parameter is optional, so `new CronExpression()` creates `"* * * * *"`. Arguments are assigned through the properties, so they are validated at construction.

| Parameter   | Type      | Default | Sets        |
| ----------- | --------- | ------- | ----------- |
| `minute`    | `string?` | `"*"`   | `Minute`    |
| `hour`      | `string?` | `"*"`   | `Hour`      |
| `day`       | `string?` | `"*"`   | `Day`       |
| `month`     | `string?` | `"*"`   | `Month`     |
| `dayOfWeek` | `string?` | `"*"`   | `DayOfWeek` |

### Properties

Each property automatically validates the assigned value.

| Property    | Type     | Description           |
| ----------- | -------- | ---------------------- |
| `Minute`    | `string` | Minute component       |
| `Hour`      | `string` | Hour component         |
| `Day`       | `string` | Day of month component |
| `Month`     | `string` | Month component        |
| `DayOfWeek` | `string` | Day of week component  |

### Methods

The type's three public methods:

| Method                                                              | Description                                                                                     |
| ---------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `string ToCronExpression()`                                         | Returns the formatted 5‑field cron string and confirms that an explicit day is valid for the specified month. |
| `static CronExpression Parse(string value)`                         | Parses a cron string; throws on invalid input.                                                  |
| `static bool TryParse(string value, out CronExpression? expression)` | Parses without throwing.                                                                         |

## Cron format

This library uses the standard 5‑field format that Kubernetes CronJobs expect.

| Field     | Property    | Allowed examples                    | Step syntax | Valid range           |
| --------- | ----------- | ----------------------------------- | ----------- | --------------------- |
| Minute    | `Minute`    | `*`, `0`, `*/5`, `0,15,30`, `10-20` | Yes         | 0–59                  |
| Hour      | `Hour`      | `*`, `0`, `*/4`, `9,17`, `6-18`     | Yes         | 0–23                  |
| Day (DOM) | `Day`       | `*`, `1`, `1,15`, `1-31`, `*/2`     | Yes         | 1–31                  |
| Month     | `Month`     | `*`, `1`, `1,6,12`, `1-12`, `*/3`   | Yes         | 1–12                  |
| DayOfWeek | `DayOfWeek` | `*`, `0`, `1-5`, `0,6`, `7`         | **No**      | 0–7 (Sunday–Saturday) |

If a value is outside of these ranges or invalid for its position, the property setter throws a `FormatException`, `ArgumentOutOfRangeException`, or `NotSupportedException`.

A comma‑separated list is validated one element at a time, so each element may itself be a single value or a range, and - for every field except `DayOfWeek`, which does not support step syntax at all (see below) - a step as well. For example, `5-10,15-31` and `0,30,*/15` are both accepted.

:::note[DayOfWeek accepts 7 as an alias for Sunday]

Per the cron specification, both `0` and `7` mean Sunday in the `DayOfWeek` field. `7` is stored and reflected exactly as assigned - setting `DayOfWeek = "7"` keeps `DayOfWeek == "7"`, and `ToCronExpression()` reflects that. `7` remains a distinct value through validation and storage, which is what lets a range correctly span into Sunday: `DayOfWeek = "5-7"` means Friday, Saturday, and Sunday, the same as `"5,6,0"` would. The equivalence between `0` and `7` is applied only when matching an actual date (`WillRunOn`, `GetNextExecution`) - an actual Sunday satisfies either representation, alone, in a list, or at the end of a range.

:::

### Unsupported syntax

This library implements a deliberately small grammar. The only characters accepted in any field are `*`, digits, `,`, `-`, and `/`. The following syntax is valid in other cron dialects but is **not** supported here:

| Not supported          | Examples                        |
| ---------------------- | -------------------------------- |
| Non‑numeric names      | `JAN`, `DEC`, `MON`, `SUN`      |
| Macros / nicknames     | `@daily`, `@hourly`, `@reboot`  |
| Quartz special tokens  | `?`, `L`, `W`, `#`              |
| Range combined with a step | `1-10/2`                    |
| A seconds or year field | `0 * * * * *` (six fields)     |

Expressions always have exactly five fields; there is no seconds mode.

:::important[Invalid Day of Month]

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

## Validation behavior

Setting any field automatically validates that field's syntax and numeric ranges. Validation errors occur immediately at assignment, or when `ToCronExpression()` confirms that a specified day/month combination is valid.

| Scenario                                                        | Exception                                                 | Notes                                                              |
| ------------------------------------------------------------------ | --------------------------------------------------------- | ------------------------------------------------------------------- |
| Invalid syntax or non‑numeric token (`a`, `1a`, `1-2-3`, `1/2/3`)   | `FormatException`                                          | Check for names such as `MON`, macros such as `@daily`, a range combined with a step (`1-10/2`), or a step with more than one `/` (`1/2/3`). |
| Value outside the allowed range (`8` for `DayOfWeek`)               | `ArgumentOutOfRangeException`                              | `DayOfWeek`'s valid range is 0–7 (`7` is legal in its own right - see the note above); `8` and above are genuinely out of range. |
| Range start not less than end (`6-3`, `5-5`)                       | `ArgumentOutOfRangeException`                              | To express a single value, assign that value directly rather than as a range. |
| Step interval of `0`, or larger than the field's maximum            | `ArgumentOutOfRangeException`                              | An interval must be at least `1` and no greater than the field's own maximum, e.g. `EveryXMinutes(60)`-style values fail. |
| Step syntax on `DayOfWeek` (`*/1`, `1/2`)                          | `NotSupportedException`                                    | Use a list (`1,3,5`) or a range (`1-5`) instead.                    |
| Day invalid for every selected month                                | `ArgumentOutOfRangeException` (from `ToCronExpression()`)  | Adjust the day, or widen the month field to include one that has that day. |

Every syntax error - including a malformed step like `1/2/3` - is caught by the property setter itself, before `ToCronExpression()` is ever called. The only validation `ToCronExpression()` performs on its own is the day/month cross-check described above.

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

## Common recipes

A few ready-to-use expressions for common schedules:

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
