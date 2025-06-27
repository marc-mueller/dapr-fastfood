using System.Text;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace FastFood.Observability.Common.Exporters.AnsiConsole
{
    /// <summary>
    /// ANSI colored console exporter for OpenTelemetry logs
    /// </summary>
    public class AnsiConsoleLogRecordExporter : ConsoleExporter<LogRecord>
    {
        private bool _disposed;
        private readonly OpenTelemetryLoggerOptions _loggerOptions;
        private readonly AnsiConsoleTheme _theme;
        private readonly bool _useUtcTimestamp;

        /// <summary>
        /// Creates a new instance of AnsiConsoleLogRecordExporter
        /// </summary>
        public AnsiConsoleLogRecordExporter(
            ConsoleExporterOptions options, 
            OpenTelemetryLoggerOptions loggerOptions, 
            AnsiConsoleTheme theme = null,
            bool useUtcTimestamp = true)
            : base(options)
        {
            _loggerOptions = loggerOptions;
            _theme = theme ?? AnsiConsoleTheme.Literate;
            _useUtcTimestamp = useUtcTimestamp;
        }

        /// <summary>
        /// Exports a batch of log records with ANSI colors
        /// </summary>
        public override ExportResult Export(in Batch<LogRecord> batch)
        {
            try
            {
                foreach (var record in batch)
                {
                    var levelColor = GetLevelColor(record.LogLevel);
                    var sb = new StringBuilder();

                    // Format timestamp
                    sb.Append(_theme.GetStyle(LogFormatElement.Timestamp));
                    var timestamp = _useUtcTimestamp ? record.Timestamp.ToUniversalTime() : record.Timestamp.ToLocalTime();
                    sb.Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss "));
                    
                    // Format log level with appropriate color
                    sb.Append(levelColor);
                    sb.Append(FormatLevel(record.LogLevel));
                    sb.Append(' ');
                    
                    // Format category
                    sb.Append(_theme.GetStyle(LogFormatElement.SecondaryText));
                    sb.Append('[');
                    sb.Append(FormatCategory(record.CategoryName));
                    sb.Append("] ");
                    
                    // Format message - use only formatted message to avoid duplicates
                    sb.Append(_theme.GetStyle(LogFormatElement.Text));
                    if (_loggerOptions.IncludeFormattedMessage && !string.IsNullOrEmpty(record.FormattedMessage))
                    {
                        sb.Append(record.FormattedMessage);

                        // Check if there are non-template attributes to include
                        if (record.Attributes != null && record.Attributes.Count > 0)
                        {
                            // Find the original format template if it exists
                            string originalFormat = null;
                            var formatAttr = record.Attributes.FirstOrDefault(attr => attr.Key == "{OriginalFormat}");
                            if (!string.IsNullOrEmpty(formatAttr.Key))
                            {
                                originalFormat = formatAttr.Value?.ToString();
                            }

                            // Only include attributes not already in the message template
                            var attributesToInclude = record.Attributes
                                .Where(kvp => kvp.Key != "{OriginalFormat}" &&
                                            (originalFormat == null || !originalFormat.Contains("{" + kvp.Key + "}")))
                                .ToList();

                            if (attributesToInclude.Any())
                            {
                                sb.Append(_theme.GetStyle(LogFormatElement.TertiaryText));
                                sb.Append(" {");
                                bool first = true;
                                
                                foreach (var kvp in attributesToInclude)
                                {
                                    if (!first)
                                    {
                                        sb.Append(", ");
                                    }
                                    
                                    sb.Append(_theme.GetStyle(LogFormatElement.AttributeKey));
                                    sb.Append(kvp.Key);
                                    sb.Append(_theme.GetStyle(LogFormatElement.TertiaryText));
                                    sb.Append(": ");
                                    sb.Append(_theme.GetStyle(LogFormatElement.AttributeValue));
                                    sb.Append(kvp.Value?.ToString() ?? "null");
                                    
                                    first = false;
                                }
                                
                                sb.Append(_theme.GetStyle(LogFormatElement.TertiaryText));
                                sb.Append('}');
                            }
                        }
                    }
                    else
                    {
                        sb.Append(record.Body);
                        
                        // Add all attributes if we're not using formatted messages
                        if (record.Attributes != null && record.Attributes.Count > 0)
                        {
                            sb.Append(_theme.GetStyle(LogFormatElement.TertiaryText));
                            sb.Append(" {");
                            bool first = true;
                            
                            foreach (var kvp in record.Attributes)
                            {
                                if (!first)
                                {
                                    sb.Append(", ");
                                }
                                
                                sb.Append(_theme.GetStyle(LogFormatElement.AttributeKey));
                                sb.Append(kvp.Key);
                                sb.Append(_theme.GetStyle(LogFormatElement.TertiaryText));
                                sb.Append(": ");
                                sb.Append(_theme.GetStyle(LogFormatElement.AttributeValue));
                                sb.Append(kvp.Value?.ToString() ?? "null");
                                
                                first = false;
                            }
                            
                            sb.Append(_theme.GetStyle(LogFormatElement.TertiaryText));
                            sb.Append('}');
                        }
                    }
                    
                    // Add trace context if present
                    if (record.TraceId != default || record.SpanId != default)
                    {
                        sb.Append(_theme.GetStyle(LogFormatElement.TertiaryText));
                        sb.Append(" [");
                        sb.Append(_theme.GetStyle(LogFormatElement.TraceId));
                        sb.Append("TraceId:");
                        sb.Append(record.TraceId);
                        sb.Append(_theme.GetStyle(LogFormatElement.TertiaryText));
                        sb.Append(" ");
                        sb.Append(_theme.GetStyle(LogFormatElement.SpanId));
                        sb.Append("SpanId:");
                        sb.Append(record.SpanId);
                        sb.Append(_theme.GetStyle(LogFormatElement.TertiaryText));
                        sb.Append(']');
                    }
                    
                    // Add exception if present
                    if (record.Exception != null)
                    {
                        sb.AppendLine();
                        sb.Append(_theme.GetStyle(LogFormatElement.Exception));
                        sb.Append(record.Exception.ToString());
                    }
                    
                    // Reset colors and print
                    sb.Append(_theme.Reset);
                    Console.WriteLine(sb.ToString());
                }

                return ExportResult.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to export batch: {ex.Message}");
                return ExportResult.Failure;
            }
        }

        private string GetLevelColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => _theme.GetStyle(LogFormatElement.LogLevelTrace),
                LogLevel.Debug => _theme.GetStyle(LogFormatElement.LogLevelDebug),
                LogLevel.Information => _theme.GetStyle(LogFormatElement.LogLevelInformation),
                LogLevel.Warning => _theme.GetStyle(LogFormatElement.LogLevelWarning),
                LogLevel.Error => _theme.GetStyle(LogFormatElement.LogLevelError),
                LogLevel.Critical => _theme.GetStyle(LogFormatElement.LogLevelCritical),
                _ => _theme.GetStyle(LogFormatElement.LogLevel)
            };
        }

        private string FormatLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => "TRACE",
                LogLevel.Debug => "DEBUG",
                LogLevel.Information => " INFO",
                LogLevel.Warning => " WARN",
                LogLevel.Error => "ERROR",
                LogLevel.Critical => " CRIT",
                _ => level.ToString().ToUpperInvariant().PadRight(5)
            };
        }

        private string FormatCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return "Default";
            
            // For long category names, try to extract just the class name
            if (category.Length > 30 && category.Contains('.'))
            {
                var parts = category.Split('.');
                return parts[parts.Length - 1];
            }
            
            return category;
        }

        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                this._disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}