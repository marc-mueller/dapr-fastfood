using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace FastFood.Observability.Common.Exporters.AnsiConsole
{
    /// <summary>
    /// Extension methods for adding ANSI colored console exporter to OpenTelemetry logging
    /// </summary>
    public static class AnsiConsoleExporterLoggingExtensions
    {
        /// <summary>
        /// Adds an ANSI colored console exporter with the default theme (Literate)
        /// </summary>
        public static OpenTelemetryLoggerOptions AddAnsiConsoleExporter(this OpenTelemetryLoggerOptions loggerOptions)
            => AddAnsiConsoleExporter(loggerOptions, null, null);

        /// <summary>
        /// Adds an ANSI colored console exporter with the specified theme
        /// </summary>
        public static OpenTelemetryLoggerOptions AddAnsiConsoleExporter(
            this OpenTelemetryLoggerOptions loggerOptions,
            AnsiConsoleTheme theme)
            => AddAnsiConsoleExporter(loggerOptions, configure: null, theme);

        /// <summary>
        /// Adds an ANSI colored console exporter with custom configuration and optional theme
        /// </summary>
        public static OpenTelemetryLoggerOptions AddAnsiConsoleExporter(
            this OpenTelemetryLoggerOptions loggerOptions,
            Action<ConsoleExporterOptions> configure,
            AnsiConsoleTheme theme = null,
            bool useUtcTimestamp = true)
        {
            if (loggerOptions == null)
            {
                throw new ArgumentNullException(nameof(loggerOptions));
            }

            var options = new ConsoleExporterOptions();
            configure?.Invoke(options);
            return loggerOptions.AddProcessor(
                new SimpleLogRecordExportProcessor(
                    new AnsiConsoleLogRecordExporter(options, loggerOptions, theme, useUtcTimestamp)));
        }
    }
}