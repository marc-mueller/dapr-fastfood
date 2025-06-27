using System.Diagnostics.Metrics;
using System.Reflection;
using FastFood.Observability.Common.Exporters.AnsiConsole;
using FastFood.Observability.Common.Exporters.JsonConsole;
using FastFood.Observability.Common.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;
using OpenTelemetry.Instrumentation.SqlClient;
using OpenTelemetry.Instrumentation.StackExchangeRedis;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FastFood.Observability.Common
{
    public static class DiagnosticsServiceCollectionExtension
    {
        public static IServiceCollection AddObservability<TInterfaceObservability, TObservability>(
            this IServiceCollection services, ObservabilityOptions observabilityOptions,
            Func<ObservabilityOptions, TObservability> observabilityFactory)
            where TObservability :ObservabilityBase, IObservability, TInterfaceObservability 
            where TInterfaceObservability : class
        {
            ArgumentNullException.ThrowIfNull(observabilityOptions);

            TObservability observability = observabilityFactory(observabilityOptions);   
            
            services.AddSingleton(observability);
            services.AddSingleton<IObservability>(observability);
            services.AddSingleton<TInterfaceObservability>(observability);

            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(observabilityOptions.ServiceName)
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector()
                .AddContainerDetector();


            Sampler sampler = observabilityOptions.SamplerType switch
            {
                SamplerType.AlwaysOnSampler => new AlwaysOnSampler(),
                SamplerType.AlwaysOffSampler => new AlwaysOffSampler(),
                SamplerType.TraceIdRatioBasedSampler =>
                    new TraceIdRatioBasedSampler(observabilityOptions.SamplingRatio),
                // Add more cases as needed
                _ => new AlwaysOnSampler() // Default case
            };

            // Configure OpenTelemetry tracing & metrics with auto-start using the
            // AddOpenTelemetry extension from OpenTelemetry.Extensions.Hosting.
            services.AddOpenTelemetry()
                .ConfigureResource(builder => ConfigureResource(builder, observabilityOptions.ServiceName))
                .WithTracing(builder =>
                {
                    ConfigureTracing(builder, services, observabilityOptions, observabilityOptions.ServiceName, resourceBuilder, sampler);
                })
                .WithMetrics(builder =>
                {
                    ConfigureObservabilityMetrics(builder, observabilityOptions, resourceBuilder);
                });

            services.AddLogging(builder =>
            {
                ConfigureObservabilityLogging(builder, observabilityOptions);
            });

            return services;
        }

        private static void ConfigureResource(ResourceBuilder resourceBuilder, string serviceName)
        {
            // Build a resource configuration action to set service information.
            resourceBuilder.AddService(
                serviceName: serviceName,
                serviceVersion: Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName);
        }

        public static void ConfigureObservabilityLogging(this ILoggingBuilder builder, ObservabilityOptions observabilityOptions)
        {
            // Clear default logging providers used by WebApplication host.
            builder.ClearProviders();

            // Set the log levels
            builder.SetMinimumLevel(observabilityOptions.LogLevels.Default);

            // Load filters from configuration.

            foreach (var filter in observabilityOptions.LogLevels.Filters)
            { 
                builder.AddFilter(filter.Key, filter.Value);
            }

            // Configure OpenTelemetry Logging.
            builder.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.ParseStateValues = true;

                // Note: See appsettings.json Logging:OpenTelemetry section for configuration.

                var resourceBuilder = ResourceBuilder.CreateDefault();
                ConfigureResource(resourceBuilder, observabilityOptions.ServiceName);
                options.SetResourceBuilder(resourceBuilder);

                switch (observabilityOptions.UseLogExporter)
                {
                    case LogExporter.Otlp:
                        options.AddOtlpExporter(o =>
                        {
                            o.Endpoint = observabilityOptions.OtlpExporter.Endpoint;
                            o.ExportProcessorType = observabilityOptions.OtlpExporter.ExportProcessorType;
                            o.HttpClientFactory = observabilityOptions.OtlpExporter.HttpClientFactory;
                            o.BatchExportProcessorOptions = observabilityOptions.OtlpExporter.BatchExportProcessorOptions;
                            o.Headers = observabilityOptions.OtlpExporter.Headers;
                            o.Protocol = observabilityOptions.OtlpExporter.Protocol;
                            o.TimeoutMilliseconds = observabilityOptions.OtlpExporter.TimeoutMilliseconds;
                        });
                        break;
                    case LogExporter.Console:
                        options.AddConsoleExporter();
                        break;
                    case LogExporter.JsonConsole:
                        options.AddJsonConsoleExporter();
                        break;
                    case LogExporter.AnsiConsole:
                        options.AddAnsiConsoleExporter();
                        break;
                    case LogExporter.OtlpAndConsole:
                        options.AddOtlpExporter(o =>
                        {
                            o.Endpoint = observabilityOptions.OtlpExporter.Endpoint;
                            o.ExportProcessorType = observabilityOptions.OtlpExporter.ExportProcessorType;
                            o.HttpClientFactory = observabilityOptions.OtlpExporter.HttpClientFactory;
                            o.BatchExportProcessorOptions = observabilityOptions.OtlpExporter.BatchExportProcessorOptions;
                            o.Headers = observabilityOptions.OtlpExporter.Headers;
                            o.Protocol = observabilityOptions.OtlpExporter.Protocol;
                            o.TimeoutMilliseconds = observabilityOptions.OtlpExporter.TimeoutMilliseconds;
                        });
                        options.AddConsoleExporter();
                        break;
                    case LogExporter.OtlpAndJsonConsole:
                        options.AddOtlpExporter(o =>
                        {
                            o.Endpoint = observabilityOptions.OtlpExporter.Endpoint;
                            o.ExportProcessorType = observabilityOptions.OtlpExporter.ExportProcessorType;
                            o.HttpClientFactory = observabilityOptions.OtlpExporter.HttpClientFactory;
                            o.BatchExportProcessorOptions = observabilityOptions.OtlpExporter.BatchExportProcessorOptions;
                            o.Headers = observabilityOptions.OtlpExporter.Headers;
                            o.Protocol = observabilityOptions.OtlpExporter.Protocol;
                            o.TimeoutMilliseconds = observabilityOptions.OtlpExporter.TimeoutMilliseconds;
                        });
                        options.AddJsonConsoleExporter();
                        break;
                    case LogExporter.OtlpAndAnsiConsole:
                        options.AddOtlpExporter(o =>
                        {
                            o.Endpoint = observabilityOptions.OtlpExporter.Endpoint;
                            o.ExportProcessorType = observabilityOptions.OtlpExporter.ExportProcessorType;
                            o.HttpClientFactory = observabilityOptions.OtlpExporter.HttpClientFactory;
                            o.BatchExportProcessorOptions = observabilityOptions.OtlpExporter.BatchExportProcessorOptions;
                            o.Headers = observabilityOptions.OtlpExporter.Headers;
                            o.Protocol = observabilityOptions.OtlpExporter.Protocol;
                            o.TimeoutMilliseconds = observabilityOptions.OtlpExporter.TimeoutMilliseconds;
                        });
                        options.AddAnsiConsoleExporter();
                        break;
                    default:
                        break;
                }
            });
        }

        private static void ConfigureObservabilityMetrics(this MeterProviderBuilder builder, ObservabilityOptions observabilityOptions, ResourceBuilder resourceBuilder)
        {
            // Metrics

            // Ensure the MeterProvider subscribes to any custom Meters.
            builder
                .AddMeter(observabilityOptions.ServiceName)
                .SetResourceBuilder(resourceBuilder)
                //.SetExemplarFilter(new TraceBasedExemplarFilter())
                .AddRuntimeInstrumentation();

            if (observabilityOptions.EnableAspNetCoreInstrumentation)
            {
                builder.AddAspNetCoreInstrumentation();
            }
            
            if (observabilityOptions.EnableHttpClientInstrumentation)
            {
                builder.AddHttpClientInstrumentation();
            }

            switch (observabilityOptions.HistogramAggregation)
            {
                case HistogramAggregation.Exponential:
                    builder.AddView(instrument =>
                    {
                        return instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
                            ? new Base2ExponentialBucketHistogramConfiguration()
                            : null;
                    });
                    break;
                default:
                    // Explicit bounds histogram is the default.
                    // No additional configuration necessary.
                    break;
            }

            switch (observabilityOptions.UseMetricsExporter)
            {
                case MetricsExporter.Prometheus:
                    builder.AddPrometheusExporter(prometheusOptions =>
                    {
                    });
                    break;
                case MetricsExporter.Otlp:
                    builder.AddOtlpExporter(o =>
                    {
                        o.Endpoint = observabilityOptions.OtlpExporter.Endpoint;
                        o.ExportProcessorType = observabilityOptions.OtlpExporter.ExportProcessorType;
                        o.HttpClientFactory = observabilityOptions.OtlpExporter.HttpClientFactory;
                        o.BatchExportProcessorOptions = observabilityOptions.OtlpExporter.BatchExportProcessorOptions;
                        o.Headers = observabilityOptions.OtlpExporter.Headers;
                        o.Protocol = observabilityOptions.OtlpExporter.Protocol;
                        o.TimeoutMilliseconds = observabilityOptions.OtlpExporter.TimeoutMilliseconds;
                    });
                    break;
                case MetricsExporter.Console:
                    builder.AddConsoleExporter();
                    break;
                default:
                    break;
            }
        }

        private static void ConfigureTracing(this TracerProviderBuilder builder, IServiceCollection services,
            ObservabilityOptions observabilityOptions, string activitySourceName,
            ResourceBuilder resourceBuilder, Sampler sampler)
            
        {
            // Tracing

            // remove health and metrics from traces
            builder.AddProcessor<HealthOrMetricsFilterProcessor>();

            // Ensure the TracerProvider subscribes to any custom ActivitySources.
            builder
                .AddSource(activitySourceName)
                .SetResourceBuilder(resourceBuilder)
                .SetSampler(sampler)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation();

            if (observabilityOptions
                .EnableSqlClientInstrumentation) // issue when active in combination with EF core instrumentation --> https://github.com/open-telemetry/opentelemetry-dotnet-contrib/issues/1764
            {
                services.Configure<SqlClientTraceInstrumentationOptions>(o =>
                {
                    o.Filter = observabilityOptions.SqlClientInstrumentation.Filter;
                    o.SetDbStatementForText = observabilityOptions.SqlClientInstrumentation.SetDbStatementForText;
                    o.RecordException = observabilityOptions.SqlClientInstrumentation.RecordException;
                    o.Enrich = observabilityOptions.SqlClientInstrumentation.Enrich;
                    o.SetDbStatementForStoredProcedure = observabilityOptions.SqlClientInstrumentation.SetDbStatementForStoredProcedure;
                    o.EnableConnectionLevelAttributes = observabilityOptions.SqlClientInstrumentation.EnableConnectionLevelAttributes;
                });
                builder.AddSqlClientInstrumentation();
            }

            if (observabilityOptions.EnableEntityFrameworkInstrumentation)
            {
                services.Configure<EntityFrameworkInstrumentationOptions>(o =>
                {
                    o.Filter = observabilityOptions.EntityFrameworkInstrumentation.Filter;
                    o.SetDbStatementForText = observabilityOptions.EntityFrameworkInstrumentation.SetDbStatementForText;
                    o.SetDbStatementForStoredProcedure = observabilityOptions.EntityFrameworkInstrumentation.SetDbStatementForStoredProcedure;
                    o.EnrichWithIDbCommand = observabilityOptions.EntityFrameworkInstrumentation.EnrichWithIDbCommand;
                });
                builder.AddEntityFrameworkCoreInstrumentation();
            }

            if (observabilityOptions.EnableRedisInstrumentation)
            {
                services.Configure<StackExchangeRedisInstrumentationOptions>(o =>
                {
                    o.Enrich = observabilityOptions.StackExchangeRedisInstrumentation.Enrich;
                    o.FlushInterval = observabilityOptions.StackExchangeRedisInstrumentation.FlushInterval;
                    o.SetVerboseDatabaseStatements = observabilityOptions.StackExchangeRedisInstrumentation.SetVerboseDatabaseStatements;
                    o.EnrichActivityWithTimingEvents = observabilityOptions.StackExchangeRedisInstrumentation.EnrichActivityWithTimingEvents;
                });
                builder.AddRedisInstrumentation();
            }

            // Use IConfiguration binding for AspNetCore instrumentation options.
            if (observabilityOptions.EnableAspNetCoreInstrumentation)
            {
                services.Configure<AspNetCoreTraceInstrumentationOptions>(o =>
                {
                    o.Filter = observabilityOptions.AspNetCoreInstrumentation.Filter;
                    o.RecordException = observabilityOptions.AspNetCoreInstrumentation.RecordException;
                    o.EnrichWithException = observabilityOptions.AspNetCoreInstrumentation.EnrichWithException;
                    o.EnrichWithHttpRequest = observabilityOptions.AspNetCoreInstrumentation.EnrichWithHttpRequest;
                    o.EnrichWithHttpResponse = observabilityOptions.AspNetCoreInstrumentation.EnrichWithHttpResponse;
                });
            }

            switch (observabilityOptions.UseTracingExporter)
            {
                case TracingExporter.Zipkin:
                    builder.AddZipkinExporter();

                    builder.ConfigureServices(services =>
                    {
                        // Use IConfiguration binding for Zipkin exporter options.
                        services.Configure<ZipkinExporterOptions>(o =>
                        {
                            o.Endpoint = observabilityOptions.ZipkinExporter.Endpoint;
                            o.ExportProcessorType = observabilityOptions.ZipkinExporter.ExportProcessorType;
                            o.HttpClientFactory  = observabilityOptions.ZipkinExporter.HttpClientFactory;
                            o.BatchExportProcessorOptions = observabilityOptions.ZipkinExporter.BatchExportProcessorOptions;
                            o.UseShortTraceIds = observabilityOptions.ZipkinExporter.UseShortTraceIds;
                            o.MaxPayloadSizeInBytes = observabilityOptions.ZipkinExporter.MaxPayloadSizeInBytes;
                        });
                    });
                    break;

                case TracingExporter.Otlp:
                    builder.AddOtlpExporter(o =>
                    {
                        o.Endpoint = observabilityOptions.OtlpExporter.Endpoint;
                        o.ExportProcessorType = observabilityOptions.OtlpExporter.ExportProcessorType;
                        o.HttpClientFactory = observabilityOptions.OtlpExporter.HttpClientFactory;
                        o.BatchExportProcessorOptions = observabilityOptions.OtlpExporter.BatchExportProcessorOptions;
                        o.Headers = observabilityOptions.OtlpExporter.Headers;
                        o.Protocol = observabilityOptions.OtlpExporter.Protocol;
                        o.TimeoutMilliseconds = observabilityOptions.OtlpExporter.TimeoutMilliseconds;
                    });
                    break;

                case TracingExporter.Console:
                    builder.AddConsoleExporter();
                    break;

                default:
                    break;
            }
        }


        public static IApplicationBuilder UseObservability(this IApplicationBuilder app, ObservabilityOptions options)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(options);

            if (options.UseMetricsExporter == MetricsExporter.Prometheus)
            {
                app.UseOpenTelemetryPrometheusScrapingEndpoint();
            }

            return app;
        }
    }
}