---
sidebar_position: 5
---

# Generating Expression String

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

:::important[Day and Month Validation]

There is a single validation done when `ToCronExpression` is executed. If both the `Days` property is a single value **AND** the `Month` property is a single value, then it will be validated that the value for day is in the range of values for that month. If the value for either `Day` or `Month` is a wildcard, an increment, a range or a list, then the validation does not run. This is to prevent creating expressions that will never execute (e.g. an expression for the 31st day of February).

:::