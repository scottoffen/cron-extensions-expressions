---
sidebar_position: 4
title: Evaluating Expressions
---

This page covers how to compute when a `CronExpression` next runs, and how to test whether it matches a given moment.

| Method                                                                                 | Purpose                                                        | Returns                                            |
| -------------------------------------------------------------------------------------- | ---------------------------------------------------------------- | --------------------------------------------------- |
| `GetNextExecution(DateTime? start = null, int maxSearchYears = 10)` | The next run time strictly after `start`, or after `DateTime.Now`. | `DateTime` with seconds set to `00`.              |
| `WillRunOn(DateTime date)`                                                             | Whether the expression matches that exact moment.              | `bool`                                             |

Both operate at minute precision across all five fields. Returned times always have seconds set to `00`, and the seconds component of an input is ignored when matching.

## Matching Rules

A field matches a candidate value according to the syntax it uses - the same rules described in [Cron Expression Format](./cron-format.md):

| Pattern      | Matches                                                                        |
| ------------ | -------------------------------------------------------------------------------- |
| `*`          | Any value                                                                      |
| `a,b,c`      | Any listed value                                                               |
| `a-b`        | Any value in the inclusive range                                               |
| `start/step` | `start`, `start + step`, `start + 2×step`, … Never a value below `start`       |
| `*/step`     | The same, starting from the field's own minimum                                |

The two day fields are the exception to "each field is checked independently": when both `Day` and `DayOfWeek` are restricted, a date matches if **either** one matches. `DayOfWeek` is also the one field where a value can match without being numerically equal, since an actual Sunday satisfies both `0` and `7`. Both rules are explained in [Field semantics](./cron-format.md).

## GetNextExecution

Computes the next occurrence at minute precision. If `start` is omitted, the current local time is used.

* Increments by the smallest field that fails to match (minute → hour → day → month), resetting lower fields as needed. The two day fields are evaluated together at the day step rather than separately.
* Returns a `DateTime` with seconds set to `0` and the same `Kind` as `start` (or `Now`). No timezone conversion is performed.
* The result is always **strictly after** `start`; the method never returns `start` itself.
* A matching minute that has already begun is never returned, whether or not `start` falls partway into it. With `*/15`, a `start` of `10:15:00` and a `start` of `10:15:32` both return `10:30:00`.

```csharp
var expr = CronExpression.Parse("*/15 * * * *");

var next = expr.GetNextExecution(new DateTime(2025, 10, 22, 10, 7, 32));
// next == 2025-10-22 10:15:00
```

```csharp
var weekday630 = CronExpression.Parse("30 6 * * 1-5");

var nextWeekday = weekday630.GetNextExecution(new DateTime(2025, 10, 25, 6, 29, 0));
// Saturday in, so the next match is Monday 2025-10-27 06:30:00
```

:::warning[Daylight saving time]

Time arithmetic uses ordinary `DateTime` addition, which applies no daylight saving time rules.

When `start` has a `Kind` of `DateTimeKind.Local` and the next run lands in an hour that the local zone skips (for example 2:00 AM on a spring-forward day), the returned value may be a time that does not exist locally. When it lands in a repeated hour (for example 1:30 AM on a fall-back day), the result is ambiguous.

For predictable behavior across DST boundaries, pass a `start` with `DateTimeKind.Utc`.

:::

### Search Horizon

`GetNextExecution` walks forward until it finds a match, so an expression that can never match would otherwise search forever. Two guards prevent that.

First, the day/month combination is validated up front - the same check `ToCronExpression()` performs - so `Day = "30"` with `Month = "2"` throws immediately rather than searching at all.

Second, `maxSearchYears` bounds how far past `start` the search will look. It defaults to 10 years, always applies, and cannot be disabled. Ten years comfortably covers every legitimately satisfiable expression, including the rarest case: day 29 combined with February needs a leap year, and at a century boundary such as 1900 that can mean an 8-year gap.

Exceeding the horizon throws `CronSearchHorizonExceededException`, which carries the `Expression`, `Start`, and `MaxSearchYears` involved. Because the first guard already rejects genuinely unsatisfiable expressions, this exception should never occur in practice. If you encounter it, please [open an issue](https://github.com/scottoffen/cron-extensions-expressions/issues) - it most likely indicates a defect in the search itself rather than a problem with your expression.

## WillRunOn

Tests a single instant. The minute, hour, and month fields must each match, and the two day fields are combined by the OR rule above. Only the minute, hour, day, month, and day-of-week components of `date` are considered, so its seconds value has no effect.

```csharp
var expr = CronExpression.Parse("*/15 * * * *");
expr.WillRunOn(new DateTime(2025, 10, 22, 10, 30, 00)); // true

var weekday630 = CronExpression.Parse("30 6 * * 1-5");
weekday630.WillRunOn(new DateTime(2025, 10, 25, 6, 30, 0)); // false - Saturday
```

```csharp
// Both day fields restricted: the 1st OR any Monday
var either = CronExpression.Parse("0 9 1 * 1");

either.WillRunOn(new DateTime(2025, 10, 1, 9, 0, 0)); // true - the 1st
either.WillRunOn(new DateTime(2025, 10, 6, 9, 0, 0)); // true - a Monday
either.WillRunOn(new DateTime(2025, 10, 2, 9, 0, 0)); // false - neither
```

Unlike `GetNextExecution`, `WillRunOn` performs no validation of its own. An expression with an impossible day/month combination simply never matches.
