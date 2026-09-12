---
sidebar_position: 6
title: Execution Extensions
---

Helpers for evaluating a `CronExpression` against dates and computing the next scheduled run time.

## Overview

`ExecutionExtensions` provides two core capabilities:

* Compute the **next execution time** of a cron expression relative to a start date.
* Evaluate whether a specific **date/time matches** a cron expression.

These methods operate with minute-level precision and consider all five cron fields (minute, hour, day, month, and day of week), combining the two day fields as standard cron does.

:::important

Returned times always have seconds set to `00`, and the seconds component of an input is ignored when matching.

:::

## Method reference

| Method                                                                                   | Description                                                                                         | Parameters                                                                                                                                                                                       | Returns                                            |
| ----------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------- |
| `GetNextExecution(DateTime? start = null, int maxSearchYears = DefaultMaxSearchYears)` | Returns the next valid run time from the provided start date or `DateTime.Now` if none is provided. | `start`: optional `DateTime` representing when to begin searching. `maxSearchYears`: how many years past `start` to search before giving up, default `10` (see below), always enforced. | `DateTime` of the next execution (seconds = 00). |
| `WillRunOn(DateTime date)`                                                                 | Determines if the cron expression will run at the specified date/time.                              | `date`: the moment to test against the cron expression.                                                                                                                                          | `bool` — `true` if the expression matches that time. |

## GetNextExecution

Computes the next occurrence at minute precision. If `start` is omitted, the current local time (`DateTime.Now`) is used. If the expression matches exactly at `start`, the method advances by one minute to find the *next* scheduled execution.

### Behavior details

* Increments by the smallest field that fails to match (minute → hour → day → month), resetting lower fields as needed. Day-of-month and day-of-week are evaluated together at the day step rather than separately, using the same rule as [Day and day of week](#day-and-day-of-week).
* Returns a `DateTime` with seconds set to `0` and the same `Kind` as `start` (or `Now`).
* Uses the date's own timezone or kind; no conversion is performed.
* The result is always **strictly after** `start`. The method never returns `start` itself.
* That comparison is made at full precision, including seconds. If `start` falls partway through a minute that the expression matches, the already-started minute is treated as past and the search continues to the following match. With `*/15`, a `start` of `10:15:32` returns `10:30:00`, not `10:15:00`.

:::warning[Daylight saving time]

Time arithmetic uses ordinary `DateTime` addition, which applies no daylight saving time rules.

When `start` has a `Kind` of `DateTimeKind.Local` and the next run lands in an hour that the local zone skips (for example 2:00 AM on a spring-forward day), the returned value may be a time that does not exist locally. When it lands in a repeated hour (for example 1:30 AM on a fall-back day), the result is ambiguous.

For predictable behavior across DST boundaries, pass a `start` with `DateTimeKind.Utc`.

:::

:::note[Unsatisfiable expressions and the search horizon]

`GetNextExecution` validates the day/month combination up front - the same check `ToCronExpression()` performs - so an expression that can never match, such as `Day = "30"` with `Month = "2"`, throws immediately rather than searching forever.

As a backstop, the search also has a `maxSearchYears` parameter (default `10`, exposed as `ExecutionExtensions.DefaultMaxSearchYears`) bounding how far past `start` it will look. This guard always applies and cannot be disabled. It comfortably covers every legitimately satisfiable expression - even the rarest case, day 29 combined with February, which needs a leap year and, at a century boundary such as 1900, can require up to an 8-year gap. Exceeding it throws `CronSearchHorizonExceededException`, which carries the `Expression`, `Start`, and `MaxSearchYears` involved. Seeing this exception in practice most likely indicates a defect in the search rather than a problem with the expression.

:::

## WillRunOn

Checks whether the provided date/time matches the cron expression. The minute, hour, and month fields must each match, and the two day fields are combined by the rule described in [Day and day of week](#day-and-day-of-week). Only the minute, hour, day, month, and day-of-week components of `date` are considered, so the seconds value has no effect on the result.

### Matching rules

| Pattern      | Behavior                                                                       |
| ------------ | ------------------------------------------------------------------------------ |
| `*`          | Matches any value                                                              |
| `a,b,c`      | Matches any listed value                                                       |
| `a-b`        | Matches inclusive range from `a` to `b`                                        |
| `start/step` | Matches `start`, `start + step`, `start + 2×step`, … Never matches below `start` |
| `*/step`     | Same, with `start` taken as the **field's minimum**: `0` for minute, hour, and day of week; `1` for day of month and month |

Because `*/step` counts from the field's own minimum, `*/2` in the day field selects the 1st, 3rd, 5th, and so on through the 31st, not the even-numbered days. In the minute field, where the minimum is `0`, `*/15` selects 0, 15, 30, and 45 as expected.

`DayOfWeek` is the one field where a value can match without being numerically equal: per the cron specification, `0` and `7` both mean Sunday. `CronExpression` stores `7` exactly as given (see [`CronExpression`](./cron-expressions.md)) rather than rewriting it, so it's `WillRunOn` and `GetNextExecution` that treat an actual Sunday as satisfying either representation - alone (`"7"`), in a list (`"1,7"`), or at the end of a range (`"5-7"`, meaning Friday through Sunday).

### Day and day of week

When **both** the day-of-month and day-of-week fields are restricted (neither is `*`), a date matches if **either** field matches. This is standard cron behavior, and it is what Kubernetes applies.

```csharp
// Both day fields restricted - fires on the 1st of the month OR any Monday
var either = CronExpression.Parse("0 9 1 * 1");
```

When at most one of the two is restricted, both must match, which amounts to letting the restricted field decide, since a `*` field always matches.

```csharp
// Only DayOfWeek restricted - weekdays only, as written
var weekdays = CronExpression.Parse("30 6 * * 1-5");
```

A field counts as restricted whenever it is anything other than `*`, so `*/2` is restricted even though it selects many days.

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

* **Day/DayOfWeek matching:** These two fields are combined with OR, following standard cron. See [Day and day of week](#day-and-day-of-week) below.
* **DayOfWeek's 7-for-Sunday alias:** See [Matching rules](#matching-rules) above - `0` and `7` are treated as equivalent when matching, including within lists and ranges.
* **Validation:** `WillRunOn` performs no validation of its own; invalid combinations are expected to have failed earlier, at assignment or at `ToCronExpression()`. `GetNextExecution` additionally validates the day/month combination itself and enforces a search horizon - see [Unsatisfiable expressions and the search horizon](#getnextexecution) above.
* **Local vs UTC:** Uses the provided `DateTime.Kind` and performs no timezone conversion. See the daylight saving time warning above before using a local `start`.
* **Step matching:** A step matches `start`, then every `step` values after it, and never a value below `start`. For `*/n` the start is the field's own minimum, which is `0` for minute, hour, and day of week, but `1` for day of month and month.
* **Performance:** Iterates to the next matching slot. Typical cron schedules resolve quickly even for sparse patterns.

## Testing tips

* Ensure `GetNextExecution(t)` never returns `t`.
* Verify correct rollovers (minute/hour/day boundaries).
* Include cases for each expression type: wildcard, list, range, and step.
