---
sidebar_position: 3
title: Building Expressions
---

There are four ways to construct a `CronExpression`: the constructor, direct property assignment, parsing an existing string, and the fluent helper methods. This page covers all of them.

## Choosing an Approach

Each of the five fields can be set four different ways. Pick whichever expresses your intent most directly - they all write to the same underlying property, and they can be mixed freely on one expression.

| Field     | Every N                                | Specific values | Range              | Direct          |
| --------- | -------------------------------------- | --------------- | ------------------ | --------------- |
| Minute    | `EveryMinute` / `EveryXMinutes`        | `OnMinutes`     | `RangeOfMinutes`   | `.Minute =`     |
| Hour      | `EveryHour` / `EveryXHours`            | `OnHours`       | `RangeOfHours`     | `.Hour =`       |
| Day       | `EveryDay` / `EveryXDays`              | `OnDays`        | `RangeOfDays`      | `.Day =`        |
| Month     | `EveryMonth` / `EveryXMonths`          | `OnMonths`      | `RangeOfMonths`    | `.Month =`      |
| DayOfWeek | *(not supported)*                      | `OnDaysOfWeek`  | `RangeOfWeek`      | `.DayOfWeek =`  |

:::tip[No day-of-week increment helpers]

Step syntax is not valid in the `DayOfWeek` field, so assigning something like `*/2` to it throws `NotSupportedException`. Use `OnDaysOfWeek` for a list or `RangeOfWeek` for a range instead.

Note also that the day-of-week range method is named `RangeOfWeek`, not `RangeOfDaysOfWeek`, while its list counterpart is `OnDaysOfWeek`.

:::

Every fluent method returns the same `CronExpression` instance, so calls can be chained, and each one touches only its own field - all others keep whatever value they already had.

## Constructor and Properties

Every constructor parameter is optional and defaults to `"*"`, so `new CronExpression()` is `"* * * * *"`. Arguments are assigned through the properties, which means they are validated at construction.

```csharp
// Incremental
var hourlyOnTheHalf = new CronExpression
{
    Minute = "30",
    Hour = "*",
};

// With the constructor
var weekdaysAt0630 = new CronExpression(minute: "30", hour: "6", dayOfWeek: "1-5");

// Render it
string text = weekdaysAt0630.ToCronExpression(); // "30 6 * * 1-5"
```

Direct assignment is the only way to express a field that the fluent helpers do not cover, such as a mixed list of ranges and steps:

```csharp
var mixed = new CronExpression { Minute = "0,30,*/15" };
```

`Month` and `DayOfWeek` also accept three-letter names (`JAN`-`DEC`, `SUN`-`SAT`) anywhere a numeric value is accepted, through the constructor, `Parse`, or direct assignment alike:

```csharp
var weekdaysFirstHalfOfYear = new CronExpression(month: "JAN-JUN", dayOfWeek: "MON-FRI");
weekdaysFirstHalfOfYear.Month; // "1-6" - names are translated to numeric immediately
```

See [Month and Day-of-Week Names](./cron-format.md#month-and-day-of-week-names) for the full rules.

## Parsing

`Parse` also accepts the standard crontab macros (`@daily`, `@hourly`, and the like) case-insensitively, as the entire trimmed value in place of the five fields - a macro is expanded to its five-field equivalent before the rest of parsing runs. Otherwise, `Parse` throws a `FormatException` if the string does not contain exactly five parts. Runs of consecutive spaces are tolerated, since empty entries are discarded before the parts are counted.

Each part is then assigned through the corresponding property, so `Parse` also propagates any validation error those setters raise. A string with the right number of parts but a bad value fails with `ArgumentOutOfRangeException` or `NotSupportedException` rather than `FormatException`.

```csharp
// Macro - expanded before parsing continues
CronExpression.Parse("@daily").ToCronExpression(); // "0 0 * * *"

// Strict parse - throws FormatException if not exactly 5 parts
var expr = CronExpression.Parse("0 0 * 1 *");

// Right shape, invalid value - throws ArgumentOutOfRangeException
CronExpression.Parse("0 0 * 13 *");

// Non-throwing parse - returns false for every failure above
if (!CronExpression.TryParse("invalid value", out var result))
{
    // handle error path
}
```

`TryParse` returns `false` for all of these cases and sets `expression` to `null`; it never throws. `@reboot` is not a recognized macro and fails the same way - see [Macros](./cron-format.md#macros).

## Increment Helpers

`Every*` sets a field to `*`. `EveryX*` sets it to a step, either `*/increment` or `start/increment`.

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

```csharp
// Every 5 minutes
new CronExpression().EveryXMinutes(5).ToCronExpression(); // "*/5 * * * *"

// Every 15 minutes starting at :00
new CronExpression().EveryXMinutes(start: 0, increment: 15).ToCronExpression(); // "0/15 * * * *"

// Every 6 hours
new CronExpression().EveryXHours(6).ToCronExpression(); // "* */6 * * *"

// Quarterly
new CronExpression().EveryXMonths(3).ToCronExpression(); // "* * * */3 *"
```

Remember that a `*/n` step counts from the field's own minimum, so `EveryXDays(2)` selects odd-numbered days rather than even ones. See [Cron Expression Format](./cron-format.md) for the full rule.

## List Helpers

Each `On*` method takes any number of `int` values, sorts them ascending, removes duplicates, and writes them as a comma-separated list.

| Method                                  | Field modified | Example input   | Resulting cron field |
| --------------------------------------- | -------------- | --------------- | -------------------- |
| `OnMinutes(params int[] minutes)`       | Minute         | `0, 15, 30, 45` | `0,15,30,45`         |
| `OnHours(params int[] hours)`           | Hour           | `6, 12, 18`     | `6,12,18`            |
| `OnDays(params int[] days)`             | Day of month   | `1, 15, 31`     | `1,15,31`            |
| `OnMonths(params int[] months)`         | Month          | `1, 6, 12`      | `1,6,12`             |
| `OnDaysOfWeek(params int[] daysOfWeek)` | Day of week    | `1, 3, 5`       | `1,3,5`              |

```csharp
// Top and bottom of every hour
new CronExpression().OnMinutes(0, 30).ToCronExpression(); // "0,30 * * * *"

// Quarterly, by naming the months
new CronExpression().OnMonths(1, 4, 7, 10).ToCronExpression(); // "* * * 1,4,7,10 *"

// Noon on Monday, Wednesday, and Friday
new CronExpression().OnHours(12).OnDaysOfWeek(1, 3, 5).ToCronExpression(); // "* 12 * * 1,3,5"

// Sorting and de-duplication are automatic
new CronExpression().OnHours(12, 6, 6).Hour; // "6,12"
```

`OnDaysOfWeek` treats `0` and `7` as distinct values, so `OnDaysOfWeek(0, 7)` keeps both and sorts to `"0,7"` rather than collapsing to a single `0`. Both still match Sunday.

## Range Helpers

Each `RangeOf*` method writes an inclusive `start-end` range. The start value must be **strictly less than** the end value.

| Method                               | Field modified | Example input | Resulting cron field |
| ------------------------------------ | -------------- | ------------- | -------------------- |
| `RangeOfMinutes(int start, int end)` | Minute         | `0, 30`       | `0-30`               |
| `RangeOfHours(int start, int end)`   | Hour           | `8, 17`       | `8-17`               |
| `RangeOfDays(int start, int end)`    | Day of month   | `1, 15`       | `1-15`               |
| `RangeOfMonths(int start, int end)`  | Month          | `1, 6`        | `1-6`                |
| `RangeOfWeek(int start, int end)`    | Day of week    | `1, 5`        | `1-5`                |

```csharp
// Business hours
new CronExpression().RangeOfHours(8, 17).ToCronExpression(); // "* 8-17 * * *"

// Weekdays only
new CronExpression().RangeOfWeek(1, 5).ToCronExpression(); // "* * * * 1-5"

// First half of the year
new CronExpression().RangeOfMonths(1, 6).ToCronExpression(); // "* * * 1-6 *"
```

`RangeOfWeek` accepts `7` as the end of a range to reach Sunday, so `RangeOfWeek(5, 7)` produces `"5-7"` - Friday through Sunday.

## Chaining

Because every fluent method returns the same instance, the three families combine freely, and direct property assignment can fill in anything they do not cover.

```csharp
// Every 15 minutes during business hours, weekdays only
new CronExpression()
    .EveryXMinutes(15)
    .RangeOfHours(8, 17)
    .RangeOfWeek(1, 5)
    .ToCronExpression(); // "*/15 8-17 * * 1-5"
```

```csharp
// 08:00 every 3 days, setting the time directly
var every3DaysAt8 = new CronExpression().EveryXDays(3);
every3DaysAt8.Minute = "0";
every3DaysAt8.Hour = "8";

every3DaysAt8.ToCronExpression(); // "0 8 */3 * *"
```

## Rendering

`ToCronExpression()` returns the five-field string and is the only method that performs the day/month check described in [Cron Expression Format](./cron-format.md). That check runs only when `Day` is a single numeric value, so an expression whose day field is a list, range, or step is rendered without it.

See [Validation and Errors](./validation.md) for everything that can throw while building an expression.
