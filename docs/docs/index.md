---
sidebar_position: 1
title: CRON Expressions
---

# What is a CRON Expression?

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

:::warning[Property Validation]

Values for each property will be validated for both format and range before being assigned. An exception will be thrown if the format or value is not valid for the property. E.g. 
- minutes must be between 0 and 59
- range start values must be less than end values
- day of week does not support increment values.

:::
