---
sidebar_position: 4
---

# Modifying Expressions

Expressions can be modified by directly changing the value of a property, or by using fluent methods. All of the fluent methods are chainable.

## Increments

Using increments, expressions can be set to every unit of time, or every x units of time, and can be set to start at a specific value before executing every x units of time.

| Fluent Method         | Description                                                              |
|-----------------------|--------------------------------------------------------------------------|
| `EveryMinute()`       | The expression will be set to every minute.                              |
| `EveryHour()`         | The expression will be set to every hour.                                |
| `EveryDay()`          | The expression will be set to every day.                                 |
| `EveryMonth()`        | The expression will be set to every month.                               |
| `EveryXMinutes(x)`    | The expression will be set to every `x` minutes.                         |
| `EveryXHours(x)`      | The expression will be set to every `x` hours.                           |
| `EveryXDays(x)`       | The expression will be set to every `x` days.                            |
| `EveryXMonths(x)`     | The expression will be set to every `x` months.                          |
| `EveryXMinutes(s, x)` | The expression will be set to every `x` minutes, starting on minute `s`. |
| `EveryXHours(s, x)`   | The expression will be set to every `x` hours, starting on hour `s`.     |
| `EveryXDays(s, x)`    | The expression will be set to every `x` days, starting on day `s`.       |
| `EveryXMonths(s, x)`  | The expression will be set to every `x` months, starting on month `s`.   |

## Ranges

Use ranges to specify a specific range of values for the given unit of time. Validation will ensure that the start value is lower than the end value, and that both values are in range for the unit of time.

| Fluent Method          | Description                                                                                                |
|------------------------|------------------------------------------------------------------------------------------------------------|
| `RangeOfMinutes(s, e)` | The expression will be set to start on minute `s` and execute every minute until `e`, inclusive.           |
| `RangeOfHours(s, e)`   | The expression will be set to start on hour `s` and execute every hour until `e`, inclusive.               |
| `RangeOfDays(s, e)`    | The expression will be set to start on day `s` and execute every day until `e`, inclusive.                 |
| `RangeOfMonths(s, e)`  | The expression will be set to start on month `s` and execute every month until `e`, inclusive.             |
| `RangeOfWeek(s, e)`    | The expression will be set to start on day of week `s` and execute every day of week until `e`, inclusive. |

## Single Values and Lists

Assign a single value or specify a list of specific values. All values will be validated to be in range, and sorted from lowest to highest.

| Fluent Method                | Description                                                                            |
|------------------------------|----------------------------------------------------------------------------------------|
| `OnMinutes(params int[])`    | The expression will be set to run on each minute specified in the parameter list.      |
| `OnHours(params int[])`      | The expression will be set to run on each hour specified in the parameter list.        |
| `OnDays(params int[])`       | The expression will be set to run on each day specified in the parameter list.         |
| `OnMonths(params int[])`     | The expression will be set to run on each month specified in the parameter list.       |
| `OnDaysOfWeek(params int[])` | The expression will be set to run on each day of week specified in the parameter list. |

## Direct Property Modification

Each property can be modified directly rather than using a fluent method. The value will be validated before being assigned to ensure that it is formatted correctly and that the values are in range for the specific property (e.g. valid minutes are 0-59, valid months are 1-12).

The best use case for direct property modification is to set a value that is not possible to do using the fluent syntax. For example, using the fluent syntax above, you can set a property to a range of values or to a list of values, but not to a list of ranged values.

In the example below, the expression is set to run on the 5th to the 10th, and on the 15th to the 31st.

```csharp
var expression = new CronExpression();
expression.Days = "5-10,15-31";
```

This can also be done via the constructor, if the values are known at the time the object is being initialized.

```csharp
var expression = new CronExpression(days: "5-10,15-31");
```

:::warning[DayOfWeek Does Not Increment]

The day of week property in a cron expression cannot be set to an increment value. Consequently, when directly setting the `DayOfWeek` property, the validator will throw an exception if the value is formatted as an increment.

:::