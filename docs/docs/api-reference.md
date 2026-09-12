---
sidebar_position: 7
title: API Reference
---

This page lists the complete public surface of `Cron.Extensions.Expressions`.

## CronExpression

The core type. It is mutable, and every assignment is validated.

### Fields

Each field is exposed as a property and can also be set through the corresponding constructor parameter. Every constructor parameter is optional and defaults to `"*"`, so `new CronExpression()` produces `"* * * * *"`. Arguments are assigned through the properties, which means they are validated at construction.

| Property    | Constructor parameter | Type     | Default | Valid range           |
| ----------- | --------------------- | -------- | ------- | --------------------- |
| `Minute`    | `minute`              | `string` | `"*"`   | 0–59                  |
| `Hour`      | `hour`                | `string` | `"*"`   | 0–23                  |
| `Day`       | `day`                 | `string` | `"*"`   | 1–31                  |
| `Month`     | `month`               | `string` | `"*"`   | 1–12                  |
| `DayOfWeek` | `dayOfWeek`           | `string` | `"*"`   | 0–7 (Sunday–Saturday) |

See [Validation and Errors](./validation.md) for what an invalid assignment throws.

### Methods

The type exposes three public methods:

| Method                                                              | Description                                                                                                   |
| ------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| `string ToCronExpression()`                                         | Returns the formatted five-field cron string, and confirms that an explicit day is valid for the selected months. |
| `static CronExpression Parse(string value)`                         | Parses a cron string; throws on invalid input.                                                                |
| `static bool TryParse(string value, out CronExpression? expression)`| Parses without throwing; returns `false` and sets `expression` to `null` on failure.                          |

## Increment Helpers

These extension methods set a field to `*` or to a step interval. See [Building Expressions](./building-expressions.md) for valid ranges and examples.

| Method                                    | Sets                            |
| ----------------------------------------- | ------------------------------- |
| `EveryMinute()`                           | `Minute = "*"`                  |
| `EveryXMinutes(int increment)`            | `Minute = "*/{increment}"`      |
| `EveryXMinutes(int start, int increment)` | `Minute = "{start}/{increment}"`|
| `EveryHour()`                             | `Hour = "*"`                    |
| `EveryXHours(int increment)`              | `Hour = "*/{increment}"`        |
| `EveryXHours(int start, int increment)`   | `Hour = "{start}/{increment}"`  |
| `EveryDay()`                              | `Day = "*"`                     |
| `EveryXDays(int increment)`               | `Day = "*/{increment}"`         |
| `EveryXDays(int start, int increment)`    | `Day = "{start}/{increment}"`   |
| `EveryMonth()`                            | `Month = "*"`                   |
| `EveryXMonths(int increment)`             | `Month = "*/{increment}"`       |
| `EveryXMonths(int start, int increment)`  | `Month = "{start}/{increment}"` |

There are no day-of-week increment methods, because step syntax is not valid in that field.

## List Helpers

These extension methods set a field to a sorted, deduplicated comma-separated list.

| Method                                  | Sets        |
| --------------------------------------- | ----------- |
| `OnMinutes(params int[] minutes)`       | `Minute`    |
| `OnHours(params int[] hours)`           | `Hour`      |
| `OnDays(params int[] days)`             | `Day`       |
| `OnMonths(params int[] months)`         | `Month`     |
| `OnDaysOfWeek(params int[] daysOfWeek)` | `DayOfWeek` |

## Range Helpers

These extension methods set a field to an inclusive `start-end` range. The `start` value must be strictly less than `end`.

| Method                               | Sets        |
| ------------------------------------ | ----------- |
| `RangeOfMinutes(int start, int end)` | `Minute`    |
| `RangeOfHours(int start, int end)`   | `Hour`      |
| `RangeOfDays(int start, int end)`    | `Day`       |
| `RangeOfMonths(int start, int end)`  | `Month`     |
| `RangeOfWeek(int start, int end)`    | `DayOfWeek` |

## Execution Helpers

These extension methods evaluate an expression against real dates. See [Evaluating Expressions](./evaluating-expressions.md) for behavior details.

| Method                                                                 | Description                                                        |
| ------------------------------------------------------------------------ | -------------------------------------------------------------------- |
| `DateTime GetNextExecution(DateTime? start = null, int maxSearchYears = 10)` | The next run time strictly after `start`, or after `DateTime.Now`. |
| `bool WillRunOn(DateTime date)`                                        | Whether the expression matches that exact moment.                  |

## Exceptions

`CronSearchHorizonExceededException` is the only exception type this library defines. Everything else it throws is a standard BCL type, listed in [Validation and Errors](./validation.md).

It is thrown by `GetNextExecution` when no match is found within its search horizon, and it inherits `InvalidOperationException`.

| Member           | Type              | Description                                                    |
| ---------------- | ----------------- | ---------------------------------------------------------------- |
| `Expression`     | `CronExpression?` | The expression being searched when the horizon was exceeded.   |
| `Start`          | `DateTime`        | The date and time the search started from.                     |
| `MaxSearchYears` | `int`             | How many years past `Start` the search was willing to look.    |
