using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Debugging;
using Serilog.Extensions.Logging.ApplicationInsights.Extensions;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Extends <see cref="ILoggerFactory"/> with methods for configuring file logging.
    /// </summary>
    public static class ApplicationInsightsLoggerExtensions
    {
        /// <summary>
        /// Adds a file logger initialized from the supplied configuration section.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="loggingConfiguration">A configuration section with file parameters.</param>
        /// <param name="applicationInsightsConfiguration">A configuration section with application insights parameters.</param>
        /// <returns>A logger factory to allow further configuration.</returns>
        public static ILoggerFactory AddApplicationInsights(
            this ILoggerFactory loggerFactory,
            IConfiguration loggingConfiguration,
            IConfiguration applicationInsightsConfiguration)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (loggingConfiguration == null) throw new ArgumentNullException(nameof(loggingConfiguration));
            if (applicationInsightsConfiguration == null) throw new ArgumentNullException(nameof(applicationInsightsConfiguration));

            var instrumentationKey = applicationInsightsConfiguration["InstrumentationKey"];
            if (string.IsNullOrWhiteSpace(instrumentationKey))
            {
                SelfLog.WriteLine("Unable to add the file logger: no InstrumentationKey was present in the configuration [ApplicationInsights:InstrumentationKey]");
                return loggerFactory;
            }

            var minimumLevel = LogLevel.Information;
            var levelSection = loggingConfiguration.GetSection("LogLevel");
            var defaultLevel = levelSection["Default"];
            if (!string.IsNullOrWhiteSpace(defaultLevel))
            {
                if (!Enum.TryParse(defaultLevel, out minimumLevel))
                {
                    SelfLog.WriteLine("The minimum level setting `{0}` is invalid", defaultLevel);
                    minimumLevel = LogLevel.Information;
                }
            }

            var levelOverrides = new Dictionary<string, LogLevel>();
            foreach (var overr in levelSection.GetChildren().Where(cfg => cfg.Key != "Default"))
            {
                LogLevel value;
                if (!Enum.TryParse(overr.Value, out value))
                {
                    SelfLog.WriteLine("The level override setting `{0}` for `{1}` is invalid", overr.Value, overr.Key);
                    continue;
                }

                levelOverrides[overr.Key] = value;
            }

            return loggerFactory.AddApplicationInsights(instrumentationKey, minimumLevel, levelOverrides);
        }

        /// <summary>
        /// Adds a file logger initialized from the supplied parameters.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="instrumentationKey">The application insights instrumentation key.</param>
        /// <param name="minimumLevel">The level below which events will be suppressed (the default is <see cref="LogLevel.Information"/>).</param>
        /// <param name="levelOverrides">A dictionary mapping logger name prefixes to minimum logging levels.</param>
        /// <returns>A logger factory to allow further configuration.</returns>
        public static ILoggerFactory AddApplicationInsights(
            this ILoggerFactory loggerFactory,
            string instrumentationKey,
            LogLevel minimumLevel = LogLevel.Information,
            IDictionary<string, LogLevel> levelOverrides = null)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (instrumentationKey == null) throw new ArgumentNullException(nameof(instrumentationKey));

            var configuration = new LoggerConfiguration()
                .MinimumLevel.Is(Conversions.MicrosoftToSerilogLevel(minimumLevel))
                .Enrich.FromLogContext()
                .Enrich.With<EventIdEnricher>()
                .WriteTo.ApplicationInsightsTraces(instrumentationKey);

            foreach (var levelOverride in levelOverrides ?? new Dictionary<string, LogLevel>())
            {
                configuration.MinimumLevel.Override(levelOverride.Key, Conversions.MicrosoftToSerilogLevel(levelOverride.Value));
            }

            var logger = configuration.CreateLogger();
            return loggerFactory.AddSerilog(logger, dispose: true);
        }
    }
}
