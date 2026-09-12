---
sidebar_position: 2
title: Cron Expression Format
---

This page describes the syntax this library accepts, along with the field semantics that apply wherever an expression is built or evaluated.

## Fields

Expressions always have exactly five fields, in the standard cron order (the same order Kubernetes CronJobs expect). There is no seconds field and no year field.

| Field     | Property    | Valid range           | Step syntax |
| --------- | ----------- | --------------------- | ----------- |
| Minute    | `Minute`    | 0–59                  | Yes         |
| Hour      | `Hour`      | 0–23                  | Yes         |
| Day (DOM) | `Day`       | 1–31                  | Yes         |
| Month     | `Month`     | 1–12                  | Yes         |
| DayOfWeek | `DayOfWeek` | 0–7 (Sunday–Saturday) | **No**      |

## Supported Syntax

The only characters accepted in `Minute`, `Hour`, and `Day` are `*`, digits, `,`, `-`, and `/`. `Month` and `DayOfWeek` accept those same characters plus the three-letter names described in [Month and Day-of-Week Names](#month-and-day-of-week-names) below.

| Syntax       | Meaning                                          | Example  | Selects                       |
| ------------ | -------------------------------------------------- | -------- | ------------------------------- |
| `*`          | Every value in the field                         | `*`      | All minutes, all hours, …      |
| `value`      | One specific value                               | `30`     | Minute 30                      |
| `a,b,c`      | Each listed value                                | `0,15,30`| Minutes 0, 15, and 30          |
| `a-b`        | An inclusive range                               | `9-17`   | Hours 9 through 17             |
| `*/step`     | Every `step`th value from the field's minimum    | `*/15`   | Minutes 0, 15, 30, 45          |
| `start/step` | Every `step`th value beginning at `start`        | `5/15`   | Minutes 5, 20, 35, 50          |
| `a-b/step`   | Every `step`th value within an inclusive range   | `9-17/3` | Hours 9, 12, 15                |

A comma-separated list is validated one element at a time, so each element may itself be a single value, a range, or - for every field except `DayOfWeek` - a step, including a step combined with a range. `5-10,15-31`, `0,30,*/15`, and `9-17/3,20-23` are all accepted.

:::note[A `*/step` counts from the field's minimum, not from zero]

The starting point is the field's own minimum, which is `0` for minute, hour, and day of week, but `1` for day of month and month. So `*/2` in the day field selects the 1st, 3rd, 5th and so on through the 31st - **not** the even-numbered days - and `*/5` in the month field selects January, June, and November. Use the explicit `start/step` form when you want to choose the starting value yourself: `2/2` gives the even days.

:::

## Month and Day-of-Week Names

`Month` and `DayOfWeek` also accept the three-letter names most cron dialects use, case-insensitively, anywhere a numeric value is accepted - alone, in a list, or on either side of a range:

| Field       | Names                                                                       |
| ----------- | ---------------------------------------------------------------------------- |
| `Month`     | `JAN`, `FEB`, `MAR`, `APR`, `MAY`, `JUN`, `JUL`, `AUG`, `SEP`, `OCT`, `NOV`, `DEC` |
| `DayOfWeek` | `SUN`, `MON`, `TUE`, `WED`, `THU`, `FRI`, `SAT`                             |

```csharp
new CronExpression(month: "JAN-JUN", dayOfWeek: "MON-FRI").ToCronExpression();
// "* * * 1-6 1-5"
```

A name is translated to its numeric equivalent the moment it's assigned. The property, and [`ToCronExpression()`](./building-expressions.md), always reflect the numeric form afterward - never the name that was given. This applies equally through the constructor, [`Parse`](./building-expressions.md), and direct assignment, since all three go through the same properties.

The interval side of a step is always numeric, matching every cron dialect that accepts names in the first place: `"JAN/3"` is rejected the same way any other malformed interval would be, since a name is never valid there.

There is no name for the `7` spelling of Sunday, only `SUN` - but `SUN` is context-sensitive. Everywhere except the end of a range it becomes `0`, matching the field's own minimum. At the end of a range it becomes `7` instead, so `"MON-SUN"` becomes `"1-7"` (Monday through Sunday) rather than the reversed `"1-0"`, which a range's start-must-be-less-than-end rule would always reject. This can only turn previously-always-invalid patterns into valid ones - a range ending in `0` was never valid to begin with, so no range that already worked can be affected. A bare `SUN`, or `SUN` at the *start* of a range (e.g. `"SUN-FRI"`), is unaffected and still becomes `0`. An explicit numeric `"0"` is never reinterpreted either way - only the name is context-sensitive, not the digit; assign `"7"` directly if that's the representation you want [reflected back](#sunday-is-0-or-7) somewhere translation wouldn't otherwise produce it.

```csharp
new CronExpression(dayOfWeek: "MON-SUN").DayOfWeek;  // "1-7" - the whole week
new CronExpression(dayOfWeek: "SAT-SUN").DayOfWeek;  // "6-7" - just the weekend
new CronExpression(dayOfWeek: "SUN-FRI").DayOfWeek;  // "0-5" - SUN at the start still means 0
```

## Macros

[`Parse`](./building-expressions.md) also accepts the standard crontab macros, case-insensitively, as the entire trimmed value in place of the five fields:

| Macro                        | Equivalent to  |
| ----------------------------- | -------------- |
| `@yearly` / `@annually`      | `0 0 1 1 *`    |
| `@monthly`                   | `0 0 1 * *`    |
| `@weekly`                    | `0 0 * * 0`    |
| `@daily` / `@midnight`       | `0 0 * * *`    |
| `@hourly`                    | `0 * * * *`    |

```csharp
CronExpression.Parse("@daily").ToCronExpression();
// "0 0 * * *"
```

A macro is expanded to its five-field equivalent before the rest of parsing runs, so the resulting instance is indistinguishable from one built from the expanded fields directly - [`ToCronExpression()`](./building-expressions.md) always returns the expanded form, never the macro that produced it.

`@reboot` is **not** supported. Every macro above is shorthand for one specific, already-representable five-field expression; `@reboot` means "once, when the scheduler starts," which isn't a recurring time and has no five-field schedule to expand into. Unlike an outright typo (`@foo`), it's a real crontab macro this library recognizes on purpose, so it throws `NotSupportedException` with a message explaining why, rather than the generic `FormatException` an unrecognized token gets.

## Unsupported Syntax

This library implements a deliberately small grammar. The following is valid in other cron dialects but is **not** supported here:

| Not supported                             | Examples                                                    |
| ------------------------------------------- | -------------------------------------------------------------- |
| `@reboot` (no five-field schedule exists) | `@reboot`                                                   |
| Quartz cron expressions                   | `?`, `L`, `W`, `#`, or a seconds or year field (`0 * * * * *`) |

## Field Semantics

The following three rules apply to every expression, no matter how it was built or when it is evaluated.

### Sunday Is 0 or 7

Per the cron specification, `0` and `7` both mean Sunday in the `DayOfWeek` field.

`7` is stored and reflected exactly as assigned - setting `DayOfWeek = "7"` keeps `DayOfWeek == "7"`, and [`ToCronExpression()`](./building-expressions.md) reflects that. Keeping `7` distinct through validation and storage is what lets a range span into Sunday: `DayOfWeek = "5-7"` means Friday, Saturday, and Sunday, the same set as `"5,6,0"`.

The equivalence is applied only when matching an actual date. An actual Sunday satisfies either representation, alone (`"7"`), in a list (`"1,7"`), or at the end of a range (`"5-7"`).

### Day Fields Combine With OR

When both `Day` and `DayOfWeek` are restricted - neither is `*` - the expression means either one **or** the other, not both together. `0 9 1 * 1` means "at 9:00 on the 1st of the month, or on any Monday", not only a Monday that happens to fall on the 1st.

When only one of the two is restricted, that field alone determines the day, since an unrestricted `*` imposes no constraint of its own. A field counts as restricted whenever it is anything other than `*`, so `*/2` is restricted even though it selects many days.

### Valid Day and Month Pairs

A day that can never occur in any selected month makes an expression unsatisfiable - day 30 in February, for example. Both [`ToCronExpression()`](./building-expressions.md) and [`GetNextExecution()`](./evaluating-expressions.md) reject that combination.

`Day` and `Month` are each expanded into the full set of values they select, and the combination fails only when **no** selected day can occur in **any** selected month:

```csharp
new CronExpression("0", "0", "31", "2,4,6").ToCronExpression();
// throws - 31 does not exist in February, April, or June

new CronExpression("0", "0", "31", "1,3").ToCronExpression();
// "0 0 31 1,3 *" - January and March both have 31 days

new CronExpression("0", "0", "31", "*").ToCronExpression();
// "0 0 31 * *" - valid, because some months have 31 days
```

February is treated as having 29 days, since a cron expression carries no year. `29 * * 2 *` is therefore accepted - it simply only runs in leap years - while day 30 or 31 in February is rejected.
