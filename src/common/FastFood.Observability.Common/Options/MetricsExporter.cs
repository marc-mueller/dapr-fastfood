namespace FastFood.Observability.Common.Options
{
    public enum MetricsExporter
    {
        None,
        Console,
        Otlp,
        Prometheus,
    }
}