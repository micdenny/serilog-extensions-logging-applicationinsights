# Serilog.Extensions.Logging.ApplicationInsights

[![Build status](https://ci.appveyor.com/api/projects/status/angsadhka3d87uv9/branch/master?svg=true)](https://ci.appveyor.com/project/micdenny/serilog-extensions-logging-applicationinsights/branch/master) [![NuGet Version](http://img.shields.io/nuget/v/Serilog.Extensions.Logging.ApplicationInsights.svg?style=flat)](https://www.nuget.org/packages/Serilog.Extensions.Logging.ApplicationInsights/)

This package makes it a one-liner - `loggerFactory.AddApplicationInsights()` - to configure top-quality application insights logging for ASP.NET Core apps.

You can get started quickly with this package, and later migrate to the full Serilog API if you need more sophisticated log configuration.

### `appsettings.json` configuration

The log level and instrumentation key can be read from JSON configuration if desired.

In `appsettings.json` add a `"Logging"` and `"ApplicationInsights"` section:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "ApplicationInsights": {
    "InstrumentationKey": "your-Instrumentation-key"
  }
}
```

And then pass the configuration sections to the `AddApplicationInsights()` method:

```csharp
loggerFactory.AddApplicationInsights(Configuration.GetSection("Logging"), Configuration.GetSection("ApplicationInsights"));
```

HINT: The `"Logging"` and `"ApplicationInsights"` sections are standard section already used by netcore.

### Using the full Serilog API

This package is opinionated, providing the most common/recommended options supported by Serilog. For more sophisticated configuration, using Serilog directly is recommended.
