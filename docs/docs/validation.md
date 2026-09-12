---
sidebar_position: 5
title: Validation and Errors
---

This page lists every exception this library throws, explains what triggers each one, and describes how to fix it.

## When Validation Happens

Validation is eager. Almost everything is caught the moment a value is assigned, rather than later when the expression is used.

| Stage                                  | What it checks                                                                                   |
| -------------------------------------- | -------------------------------------------------------------------------------------------------- |
| Property assignment                    | Syntax and numeric range for that one field. Every syntax error is caught here.                  |
| `ToCronExpression()`                   | Additionally, that the day can occur in at least one selected month.                             |
| `GetNextExecution()`                   | The same day/month check, plus the search horizon.                                               |

Because the constructor and every fluent helper assign through the properties, they all inherit the first stage. An invalid argument to `OnHours(24)` or `EveryXMinutes(0)` throws from that call, not from a later `ToCronExpression()`.

`WillRunOn()` performs no validation of its own.

`TryParse` is the one entry point that never throws - it returns `false` and sets its out parameter to `null` for every failure below.

## Exceptions

These are the exceptions the library can raise, ordered roughly by how often you are likely to encounter them:

| Scenario                                                          | Exception                                | How to fix                                                                                     |
| ------------------------------------------------------------------- | ------------------------------------------ | ------------------------------------------------------------------------------------------------ |
| Non-numeric or malformed token (`a`, `1a`, `1-2-3`, `1/2/3`)       | `FormatException`                        | Check for names (`MON`), macros (`@daily`), a range with a step (`1-10/2`), or a repeated `/`.  |
| A parsed string without exactly five fields                        | `FormatException`                        | Supply all five fields; extra spaces between them are fine.                                    |
| An empty field, such as `OnHours()` with no arguments              | `FormatException`                        | Guard against passing an empty array.                                                          |
| Value outside the field's range (`8` for `DayOfWeek`)              | `ArgumentOutOfRangeException`            | See the range column in [Cron Expression Format](./cron-format.md). `DayOfWeek` allows 0–7.    |
| Range start not less than end (`6-3`, `5-5`)                       | `ArgumentOutOfRangeException`            | To express a single value, assign it directly rather than as a range.                          |
| Step interval of `0`                                               | `ArgumentOutOfRangeException`            | An interval must be at least `1`. Reported as `Interval value 0 for <field> must be greater than 0`. |
| Step interval larger than the field's maximum (`*/60` on minute)   | `ArgumentOutOfRangeException`            | Use an interval no greater than the field's own maximum.                                       |
| Step syntax on `DayOfWeek` (`*/1`, `1/2`)                          | `NotSupportedException`                  | Use a list (`1,3,5`) or a range (`1-5`) instead.                                               |
| Day cannot occur in any selected month (day 30 with month 2)       | `ArgumentOutOfRangeException`            | Adjust the day, or widen the month field to include one that has that day.                     |
| No match found within the search horizon                           | `CronSearchHorizonExceededException`     | Should not happen for a satisfiable expression; see [the search horizon](./evaluating-expressions.md). |

## Deferred Day and Month Checks

The day/month check is the one rule that does not run on assignment, because it depends on two fields at once. It runs when `ToCronExpression()` or `GetNextExecution()` is called, and **only when `Day` is a single numeric value**.

That means how you set the day field determines whether the check applies at all:

| Day field is…                                       | Set by                                          | Check runs? |
| ----------------------------------------------------- | ------------------------------------------------- | ----------- |
| A single numeric value (`"15"`)                     | `OnDays(15)`, `Day = "15"`                      | Yes         |
| `*`, a list, a range, or a step                     | `EveryXDays`, `RangeOfDays`, `OnDays(1, 15)`, … | No          |

So `RangeOfDays(29, 31)` combined with February is not reported as an error, while `OnDays(30)` with February is. See [Field semantics](./cron-format.md) for what the check actually verifies.
