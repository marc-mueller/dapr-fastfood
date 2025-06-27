using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;
using OpenTelemetry.Instrumentation.SqlClient;
using OpenTelemetry.Instrumentation.StackExchangeRedis;

namespace FastFood.Observability.Common.Options
{
    public class ObservabilityOptions
    {
        public string ServiceName { get; set; } = string.Empty;
        public TracingExporter UseTracingExporter { get; set; } = TracingExporter.None;
        public MetricsExporter UseMetricsExporter { get; set; } = MetricsExporter.None;
        public LogExporter UseLogExporter { get; set; } = LogExporter.Console;
        public HistogramAggregation HistogramAggregation { get; set; } = HistogramAggregation.Explicit;



        public SamplerType SamplerType { get; set; } = SamplerType.AlwaysOffSampler;
        public double SamplingRatio { get; set; } = 0.01;
        public ZipkinExporterOptions ZipkinExporter { get; set; } = new ZipkinExporterOptions();
        public OtlpExporterOptions OtlpExporter { get; set; } = new OtlpExporterOptions();
        
        public LogLevelsOptions LogLevels { get; set; } = new LogLevelsOptions(){ 
            Default = LogLevel.Warning, 
            Filters = new Dictionary<string, LogLevel>(StringComparer.OrdinalIgnoreCase)
            {
                {"Microsoft", LogLevel.Error}, 
                {"System", LogLevel.Warning}
            } };


        public bool EnableEntityFrameworkInstrumentation { get; set; }

        public EntityFrameworkInstrumentationOptions EntityFrameworkInstrumentation { get; set; } = new EntityFrameworkInstrumentationOptions();
        public bool EnableRedisInstrumentation { get; set; }
        public StackExchangeRedisInstrumentationOptions StackExchangeRedisInstrumentation { get; set; } = new StackExchangeRedisInstrumentationOptions();
        public bool EnableSqlClientInstrumentation { get; set; }
        public SqlClientTraceInstrumentationOptions SqlClientInstrumentation { get; set; } = new SqlClientTraceInstrumentationOptions();
        public bool EnableAspNetCoreInstrumentation { get; set; }
        public AspNetCoreTraceInstrumentationOptions AspNetCoreInstrumentation { get; set; } = new AspNetCoreTraceInstrumentationOptions();
        public bool EnableHttpClientInstrumentation { get; set; }
    }
}