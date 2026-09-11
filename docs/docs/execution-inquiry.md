---
sidebar_position: 6
title: Execution Inquiry
---

Helpers for evaluating a `CronExpression` against dates and computing the next scheduled run time.

## Overview

`ExecutionExtensions` provides two core capabilities:

* Compute the **next execution time** of a cron expression relative to a start date.
* Evaluate whether a specific **date/time matches** a cron expression.

These methods operate with minute-level precision and apply all five cron fields (minute, hour, day, month, and day of week).

:::important
Returned times always have seconds set to `00`, and the seconds component of an input is ignored when matching.
:::

## Method reference

| Method                                     | Description                                                                                         | Parameters                                                         | Returns                                                            |
| ------------------------------------------ | --------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ | ------------------------------------------------------------------ |
| `GetNextExecution(DateTime? start = null)` | Returns the next valid run time from the provided start date or `DateTime.Now` if none is provided. | `start`: optional `DateTime` representing when to begin searching. | `DateTime` of the next execution (seconds = 00).                   |
| `WillRunOn(DateTime date)`                 | Determines if the cron expression will run at the specified date/time.                              | `date`: the moment to test against the cron expression.            | `bool` — `true` if the expression matches all fields at that time. |

## GetNextExecution

Computes the next occurrence at minute precision. If `start` is omitted, the current local time (`DateTime.Now`) is used. If the expression matches exactly at `start`, the method advances by one minute to find the *next* scheduled execution.

### Behavior details

* Increments by the smallest field that fails to match (minute → hour → day → month → day-of-week), resetting lower fields as needed.
* Returns a `DateTime` with seconds set to `0` and the same `Kind` as `start` (or `Now`).
* Uses the date's own timezone or kind; no conversion is performed.
* The result is always **strictly after** `start`. The method never returns `start` itself.
* That comparison is made at full precision, including seconds. If `start` falls partway through a minute that the expression matches, the already-started minute is treated as past and the search continues to the following match. With `*/15`, a `start` of `10:15:32` returns `10:30:00`, not `10:15:00`.

:::warning Daylight saving time
Time arithmetic uses ordinary `DateTime` addition, which applies no daylight saving time rules.

When `start` has a `Kind` of `DateTimeKind.Local` and the next run lands in an hour that the local zone skips (for example 2:00 AM on a spring-forward day), the returned value may be a time that does not exist locally. When it lands in a repeated hour (for example 1:30 AM on a fall-back day), the result is ambiguous.

For predictable behavior across DST boundaries, pass a `start` with `DateTimeKind.Utc`.
:::

:::danger Unsatisfiable expressions
`GetNextExecution` has no termination guard. An expression that can never match, such as `Day = "30"` with `Month = "2"`, causes the search to loop forever.

`ToCronExpression()` is the check that rejects impossible day/month pairs, so call it before scheduling any expression assembled from untrusted or computed input.
:::

## WillRunOn

Checks whether the provided date/time matches the cron expression. All five fields must match. Only the minute, hour, day, month, and day-of-week components of `date` are considered, so the seconds value has no effect on the result.

### Matching rules

| Pattern      | Behavior                                                                    |
| ------------ | --------------------------------------------------------------------------- |
| `*`          | Matches any value                                                           |
| `a,b,c`      | Matches any listed value                                                    |
| `a-b`        | Matches inclusive range from `a` to `b`                                     |
| `start/step` | Matches values where `(value - start) % step == 0` (for `*/n`, `start` = 0) |

## Usage examples

```csharp
var expr = CronExpression.Parse("*/15 * * * *");

// If it's 10:07 now, next is 10:15
var next = expr.GetNextExecution(new DateTime(2025, 10, 22, 10, 7, 32));
// next == 2025-10-22 10:15:00

// Check a specific instant
var run = expr.WillRunOn(new DateTime(2025, 10, 22, 10, 30, 00)); // true
```

```csharp
// Weekdays at 06:30
var weekday630 = CronExpression.Parse("30 6 * * 1-5");

weekday630.WillRunOn(new DateTime(2025, 10, 25, 6, 30, 0)); // false (Saturday)

var nextWeekday = weekday630.GetNextExecution(new DateTime(2025, 10, 25, 6, 29, 0));
// returns Monday 2025-10-27 06:30:00
```

```csharp
// First and fifteenth at 09:00, regardless of weekday
var semiMonthly9 = new CronExpression("0", "9", "1,15", "*", "*");

semiMonthly9.WillRunOn(new DateTime(2025, 10, 15, 9, 0, 0)); // true
var nextSemi = semiMonthly9.GetNextExecution(new DateTime(2025, 10, 15, 9, 0, 0));
// nextSemi == 2025-11-01 09:00:00
```

## Notes & caveats

* **Day/DayOfWeek matching:** All five fields are ANDed, including day and day of week. When both are restricted, a date must satisfy both to match. Some cron implementations OR these two fields instead, so verify against your scheduler before relying on an expression that restricts both.
* **Validation:** Assumes a valid `CronExpression`. These methods perform no validation of their own; invalid combinations are expected to have failed earlier, at assignment or at `ToCronExpression()`.
* **Local vs UTC:** Uses the provided `DateTime.Kind` and performs no timezone conversion. See the daylight saving time warning above before using a local `start`.
* **Step matching:** A step matches on the arithmetic `(value - start) % step == 0`, which is not bounded below by `start`. In `WillRunOn`, a minute field of `2/5` therefore matches any minute congruent to 2 modulo 5.
* **Performance:** Iterates to the next matching slot. Typical cron schedules resolve quickly even for sparse patterns.

## Testing tips

* Ensure `GetNextExecution(t)` never returns `t`.
* Verify correct rollovers (minute/hour/day boundaries).
* Include cases for each expression type: wildcard, list, range, and step.
