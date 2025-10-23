---
sidebar_position: 6
title: Execution Inquiry
---

Helpers for evaluating a `CronExpression` against dates and computing the next scheduled run time.

## Overview

The execution inquiry extensions provides two core capabilities:

* Compute the **next execution time** of a cron expression relative to a start date.
* Evaluate whether a specific **date/time matches** a cron expression.

These methods operate with minute-level precision and apply all five cron fields (minute, hour, day, month, and day of week).

:::important
Returned times always have seconds set to `00`.
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

## WillRunOn

Checks whether the provided date/time matches the cron expression. All five fields must match.

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

* **Day/DayOfWeek matching:** Both must match when restricted (logical AND semantics).
* **Validation:** Assumes a valid `CronExpression` (invalid date combinations should fail earlier).
* **Local vs UTC:** Uses the provided `DateTime.Kind`; no timezone adjustments.
* **Performance:** Iterates to the next matching slot. Typical cron schedules resolve quickly even for sparse patterns.

## Testing tips

* Ensure `GetNextExecution(t)` never returns `t`.
* Verify correct rollovers (minute/hour/day boundaries).
* Include cases for each expression type: wildcard, list, range, and step.
