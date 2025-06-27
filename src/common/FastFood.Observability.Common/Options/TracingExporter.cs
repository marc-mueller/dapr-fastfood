namespace FastFood.Observability.Common.Options
{
    public enum TracingExporter
    {
        None,
        Console,
        Otlp,
        Zipkin,
    }
}