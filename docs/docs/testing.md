---
sidebar_position: 7
---

# Testing Specific Dates and Times

Determine whether the expression will run at a specific date and time.

```csharp
var date1 = new DateTime(2024, 10, 10);
var date2 = new DateTime(2024, 10, 25);

var expression = new CronExpression(days: "25");

var shouldBeFalse = expression.WillRunOn(date1);
var shouldBeTrue = expression.WillRunOn(date2);
```

This example uses the [Shouldly](https://docs.shouldly.org/) assertion library.