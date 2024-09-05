# ![Logo](https://raw.githubusercontent.com/scottoffen/cron-extensions-expressions/main/cron-extensions-expressions-25x25.png) Cron.Extensions.Expressions

Easily create and parse cron expressions using a fluent syntax.

> [!IMPORTANT]
> This library currently only supports Kubernetes cron expressions.

# Usage

A cron expression is a string used to define a schedule for running tasks in Unix-like systems, particularly in the context of the cron job scheduler. Cron expressions consist of five or six fields that define specific times or intervals for executing a command or script

The typical format of a cron expression is:

```
* * * * * command_to_execute
- - - - -
| | | | |
| | | | ----- Day of the week (0 - 7) (Sunday = 0 or 7)
| | | ------- Month (1 - 12)
| | --------- Day of the month (1 - 31)
| ----------- Hour (0 - 23)
------------- Minute (0 - 59)
```

Remembering how to construct and parse these expressions can be tricky. This package provides the `CronExpression` class to simplify that.

> [!WARNING]
> Values for each property will be validated for both format and range before being assigned. An exception will be thrown if the format or value is not valid for the property. E.g. 
> - minutes must be between 0 and 59
> - range start values must be less than end values
> - day of week does not support increment values.

## Creating Expressions : Constructors

Use a parameterless constructor to create a cron expression were each value is the wildcard `*`.

```csharp
var expression = new CronExpression();
```

Use the constructor to provide specific values for each property.

```csharp
// Every five minutes every weekday from 3 to 5 pm
var expression = new CronExpression("*/5", "15-17", "*", "*", "1-5");
```

Use the constructor with named parameters to only specify certain properties. All unspecified properties will be set to `*`.

```csharp
var expression = new CronExpression(minutes: "30", dayOfWeek: "3");
```

## Creating Expressions : Parsers

Create from a string using `Parse` and `TryParse`. Per convention, `Parse` might throw an exception if the string pattern is invalid, but `TryParse` will return true or false based on whether or not the string could be parsed to a valid expression, and the result will be in the out variable.

```csharp
// 11:30 pm every weekday
var pattern = "30 23 * * 1-5";

// Throws an exception if the pattern cannot be parsed or if it is invalid
var expression = CronExpression.Parse(pattern);

// Returns true if parse was successful
var result = CronExpression.TryParse(pattern, out var expression);
```

## Modifying Expressions

Expressions can be modified by directly changing the value of a property, or by using fluent methods. All of the fluent methods are chainable.

### Increments

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

### Ranges

Use ranges to specify a specific range of values for the given unit of time. Validation will ensure that the start value is lower than the end value, and that both values are in range for the unit of time.

| Fluent Method          | Description                                                                                                |
|------------------------|------------------------------------------------------------------------------------------------------------|
| `RangeOfMinutes(s, e)` | The expression will be set to start on minute `s` and execute every minute until `e`, inclusive.           |
| `RangeOfHours(s, e)`   | The expression will be set to start on hour `s` and execute every hour until `e`, inclusive.               |
| `RangeOfDays(s, e)`    | The expression will be set to start on day `s` and execute every day until `e`, inclusive.                 |
| `RangeOfMonths(s, e)`  | The expression will be set to start on month `s` and execute every month until `e`, inclusive.             |
| `RangeOfWeek(s, e)`    | The expression will be set to start on day of week `s` and execute every day of week until `e`, inclusive. |

### Single Values and Lists

Assign a single value or specify a list of specific values. All values will be validated to be in range, and sorted from lowest to highest.

| Fluent Method                | Description                                                                            |
|------------------------------|----------------------------------------------------------------------------------------|
| `OnMinutes(params int[])`    | The expression will be set to run on each minute specified in the parameter list.      |
| `OnHours(params int[])`      | The expression will be set to run on each hour specified in the parameter list.        |
| `OnDays(params int[])`       | The expression will be set to run on each day specified in the parameter list.         |
| `OnMonths(params int[])`     | The expression will be set to run on each month specified in the parameter list.       |
| `OnDaysOfWeek(params int[])` | The expression will be set to run on each day of week specified in the parameter list. |

### Direct Property Modification

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

> [!WARNING]
> The day of week property in a cron expression cannot be set to an increment value. Consequently, when directly setting the `DayOfWeek` property, the validator will throw an exception if the value is formatted as an increment.

## Generating Expression String

The cron expression string can be generated using the `ToCronExpression()` method.

```csharp
var expression = new CronExpression(day: "5-10,15-31");
expression
    .EveryXMinutes(2)
    .RangeOfHours(8, 17)
    .OnMonths(2, 4, 6, 8, 10, 12)
    .RangeOfWeek(1, 5);

var schedule = expression.ToCronExpression();
// schedule = "*/2 8-17 5-10,15-31 2,4,6,8,10,12 1-5"
```

> [!IMPORTANT]
> There is a single validation done when `ToCronExpression` is executed. If both the `Days` property is a single value **AND** the `Month` property is a single value, then it will be validated that the value for day is in the range of values for that month. If the value for either `Day` or `Month` is a wildcard, an increment, a range or a list, then the validation does not run. This is to prevent creating expressions that will never execute (e.g. an expression for the 31st day of February).

## Getting the Next Execution DateTime

Get a `DateTime` object representing when the cron expression will run next after the provided start time.

```csharp
// runs every other hour
var expression = new CronExpression(hours: "*/2");

var halloween = new DateTime(2024, 10, 31);
var nextExecutionAfterHalloween = expression.GetNextExecution(halloween);

// Uses the current date and time of a start time is not provided
var nextExecution = expression.GetNextExecution();
```

## Testing Specific Dates and Times

Determine whether the expression will run at a specific date and time.

```csharp
var date1 = new DateTime(2024, 10, 10);
var date2 = new DateTime(2024, 10, 25);

var expression = new CronExpression(days: "25");

var shouldBeFalse = expression.WillRunOn(date1);
var shouldBeTrue = expression.WillRunOn(date2);
```

## Attribution

<a href="https://www.flaticon.com/free-icons/stopwatch" title="stopwatch icons">Stopwatch icons created by Ilham Fitrotul Hayat - Flaticon</a>