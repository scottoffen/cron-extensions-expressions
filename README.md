# Cron.Extensions.Expressions

[![docs](https://img.shields.io/badge/docs-github.io-blue)](https://scottoffen.github.io/cron-extensions-expressions)
[![NuGet](https://img.shields.io/nuget/v/Cron.Extensions.Expressiosn)](https://www.nuget.org/packages/Cron.Extensions.Expressions/)
[![MIT](https://img.shields.io/github/license/scottoffen/cron-extensions-expressions?color=blue)](./LICENSE)
[![Target1](https://img.shields.io/badge/netstandard-2.0-blue)](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
[![Target1](https://img.shields.io/badge/dotnet-5.0-blue)](https://learn.microsoft.com/en-us/dotnet/standard/frameworks)
[![Contributor Covenant](https://img.shields.io/badge/Contributor%20Covenant-2.1-blue.svg)](code_of_conduct.md)

Easily create and parse cron expressions in .NET using a fluent syntax.

> [!IMPORTANT]
> This library currently only supports Kubernetes cron expressions.

## Installation

Cron.Extensions.Expressions is available on [NuGet.org](https://www.nuget.org/packages/Cron.Extensions.Expressions/) and can be installed using a NuGet package manager or the .NET CLI.

## Usage and Support

- Check out the project documentation https://scottoffen.github.io/cron-extensions-expressions.

- Engage in our [community discussions](https://github.com/scottoffen/cron-extensions-expressions/discussions) for Q&A, ideas, and show and tell!

- **Issues created to ask "how to" questions will be closed.**

# Use Cases

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

## Contributing

We welcome contributions from the community! In order to ensure the best experience for everyone, before creating an issue or submitting a pull request, please see the [contributing guidelines](CONTRIBUTING.md) and the [code of conduct](CODE_OF_CONDUCT.md). Failure to adhere to these guidelines can result in significant delays in getting your contributions included in the project.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/scottoffen/cron-extensions-expressions/releases).

## Test Coverage

You can generate and open a test coverage report by running the following command in the project root:

```bash
pwsh ./test-coverage.ps1
```

> [!NOTE]
> This is a [Powershell](https://learn.microsoft.com/en-us/powershell/) script. You must have Powershell installed to run this command.

## License

Cron.Extensions.Expressions is licensed under the [MIT](./LICENSE) license.

## Using Cron.Extensions.Expressions? We'd Love To Hear About It!

Few thing are as satisfying as hearing that your open source project is being used and appreciated by others. Jump over to the discussion boards and [share the love](https://github.com/scottoffen/cron-extensions-expressions/discussions)!
