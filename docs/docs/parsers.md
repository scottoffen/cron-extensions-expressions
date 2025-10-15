---
sidebar_position: 3
---

# Creating Expressions via Parsers

Create from a string using `Parse` and `TryParse`. Per convention, `Parse` might throw an exception if the string pattern is invalid, but `TryParse` will return true or false based on whether or not the string could be parsed to a valid expression, and the result will be in the out variable.

```csharp
// 11:30 pm every weekday
var pattern = "30 23 * * 1-5";

// Throws an exception if the pattern cannot be parsed or if it is invalid
var expression = CronExpression.Parse(pattern);

// Returns true if parse was successful
var result = CronExpression.TryParse(pattern, out var expression);
```
