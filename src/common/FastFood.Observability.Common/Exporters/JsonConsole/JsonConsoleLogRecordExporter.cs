using System.Buffers;
using System.Text;
using System.Text.Json;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

namespace FastFood.Observability.Common.Exporters.JsonConsole
{
    public class JsonConsoleLogRecordExporter : ConsoleExporter<LogRecord>
    {
        private bool _disposed;
        private readonly OpenTelemetryLoggerOptions _loggerOptions;

        public JsonConsoleLogRecordExporter(ConsoleExporterOptions options, OpenTelemetryLoggerOptions loggerOptions)
            : base(options)
        {
            _loggerOptions = loggerOptions;
        }

        public override ExportResult Export(in Batch<LogRecord> batch)
        {
            try
            {
                var bufferWriter = new ArrayBufferWriter<byte>();

                foreach (var record in batch)
                {
                    using (var writer = new Utf8JsonWriter(bufferWriter))
                    {
                        writer.WriteStartObject();

                        writer.WriteString("Timestamp", record.Timestamp.ToUniversalTime().ToString("o"));
                        writer.WriteString("LogLevel", record.LogLevel.ToString());

                        // Only write the formatted message without duplicating information
                        if (_loggerOptions.IncludeFormattedMessage && !string.IsNullOrEmpty(record.FormattedMessage))
                        {
                            writer.WriteString("Message", record.FormattedMessage);
                            
                            // Only include attributes that aren't already in the formatted message
                            if (record.Attributes != null && record.Attributes.Count > 0)
                            {
                                // Check if there's an OriginalFormat attribute - which indicates templated logging
                                string originalFormat = null;
                                var formatAttr = record.Attributes.FirstOrDefault(attr => attr.Key == "{OriginalFormat}");
                                if (!string.IsNullOrEmpty(formatAttr.Key))
                                {
                                    originalFormat = formatAttr.Value?.ToString();
                                }

                                // If we have attributes not covered by the format, add them separately
                                var attributesToInclude = record.Attributes
                                    .Where(kvp => kvp.Key != "{OriginalFormat}" && 
                                                 (originalFormat == null || !originalFormat.Contains("{" + kvp.Key + "}")))
                                    .ToList();
                                
                                if (attributesToInclude.Any())
                                {
                                    writer.WriteStartObject("Attributes");
                                    foreach (var kvp in attributesToInclude)
                                    {
                                        writer.WriteString(kvp.Key, kvp.Value?.ToString());
                                    }
                                    writer.WriteEndObject();
                                }
                            }
                        }
                        else
                        {
                            writer.WriteString("Message", record.Body);
                            
                            // Include all attributes when not using formatted message
                            if (record.Attributes != null && record.Attributes.Count > 0)
                            {
                                writer.WriteStartObject("Attributes");
                                foreach (var kvp in record.Attributes)
                                {
                                    writer.WriteString(kvp.Key, kvp.Value?.ToString());
                                }
                                writer.WriteEndObject();
                            }
                        }

                        if (record.Exception != null)
                        {
                            writer.WriteStartObject("Exception");
                            writer.WriteString("Message", record.Exception.Message);
                            writer.WriteString("StackTrace", record.Exception.StackTrace);
                            writer.WriteEndObject();
                        }

                        writer.WriteString("TraceId", record.TraceId.ToString());
                        writer.WriteString("SpanId", record.SpanId.ToString());
                        writer.WriteString("TraceFlags", record.TraceFlags.ToString());
                        writer.WriteString("TraceState", record.TraceState);
                        writer.WriteString("CategoryName", record.CategoryName);
                        writer.WriteStartObject("EventId");
                        writer.WriteNumber("Id", record.EventId.Id);
                        writer.WriteString("Name", record.EventId.Name);
                        writer.WriteEndObject();

                        writer.WriteEndObject();
                        writer.Flush();

                        Console.WriteLine(Encoding.UTF8.GetString(bufferWriter.WrittenSpan.ToArray()));
                        bufferWriter.Clear();
                    }
                }

                return ExportResult.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to export batch: {ex.Message}");
                return ExportResult.Failure;
            }
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
