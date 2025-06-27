using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace FastFood.Observability.Common.Exporters.JsonConsole
{
    public static class JsonConsoleExporterLoggingExtensions
    {
        public static OpenTelemetryLoggerOptions AddJsonConsoleExporter(this OpenTelemetryLoggerOptions loggerOptions)
            => AddJsonConsoleExporter(loggerOptions, configure: null);

        public static OpenTelemetryLoggerOptions AddJsonConsoleExporter(this OpenTelemetryLoggerOptions loggerOptions, Action<ConsoleExporterOptions> configure)
        {
            if (loggerOptions == null)
            {
                throw new ArgumentNullException(nameof(loggerOptions));
            }


            var options = new ConsoleExporterOptions();
            configure?.Invoke(options);
            return loggerOptions.AddProcessor(new SimpleLogRecordExportProcessor(new JsonConsoleLogRecordExporter(options, loggerOptions)));
        }
    }
}