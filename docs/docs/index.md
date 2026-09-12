---
sidebar_position: 1
title: Cron.Extensions.Expressions
---

Easily create and parse cron expressions using a fluent syntax.

:::important

This library implements standard cron expressions - the five-field syntax, including names, macros, ranges, and lists, that predates Kubernetes by decades. It is fully compatible with Kubernetes CronJob schedules, but does not support Quartz cron expressions.

:::

## Package Details

* **Kubernetes CronJob compatible.** Every expression this library builds or parses is a valid Kubernetes CronJob `schedule`.
* **Broad framework support.** Targets .NET Standard 2.0 and 2.1 alongside .NET 6 through .NET 10.
* **AOT and trim friendly.** The assembly is annotated as AOT-compatible, so it can be used in Native AOT and trimmed deployments without tripping the trimming analyzers.
* **Strong-named.** The assembly is signed, which means it can be referenced from other strong-named assemblies.
* **No dependencies.** Nothing is added to your dependency graph.

## Cron Expressions

A cron expression is a string used to define a schedule for running tasks in Unix-like systems, particularly in the context of the cron job scheduler. Cron expressions consist of five fields that define specific times or intervals for executing a command or script

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

## Quick Start

A few of the most common operations:

```csharp
using Cron.Extensions.Expressions;

// Build one with the constructor
var midnight = new CronExpression(minute: "0", hour: "0");
midnight.ToCronExpression(); // "0 0 * * *"

// Or with the fluent helpers
var businessHours = new CronExpression()
    .EveryXMinutes(15)
    .RangeOfWeek(1, 5);
businessHours.ToCronExpression(); // "*/15 * * * 1-5"

// Parse an existing expression
var parsed = CronExpression.Parse("30 6 * * 1-5");

// Ask when it next runs
var next = parsed.GetNextExecution();
```

## Where to Go Next

The rest of the documentation is organized by task:

| Page | What it covers |
| ------ | ---------------- |
| [Cron Expression Format](./cron-format.md) | Which syntax this library accepts, and the field semantics that apply everywhere. |
| [Building Expressions](./building-expressions.md) | Every way to construct a `CronExpression`, including the fluent helpers. |
| [Evaluating Expressions](./evaluating-expressions.md) | Computing the next run time and testing whether a given moment matches. |
| [Validation and Errors](./validation.md) | Every exception the library throws, what causes it, and how to fix it. |
| [Common Recipes](./recipes.md) | Ready-to-use expressions for the schedules people need most often. |
| [API Reference](./api-reference.md) | The complete public surface. |
