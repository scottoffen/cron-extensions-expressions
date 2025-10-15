---
sidebar_position: 6
---

# Getting the Next Execution DateTime

Get a `DateTime` object representing when the cron expression will run next after the provided start time.

```csharp
// runs every other hour
var expression = new CronExpression(hours: "*/2");

var halloween = new DateTime(2024, 10, 31);
var nextExecutionAfterHalloween = expression.GetNextExecution(halloween);

// Uses the current date and time of a start time is not provided
var nextExecution = expression.GetNextExecution();
```