---
sidebar_position: 2
---

# Creating Expressions via Constructors

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
