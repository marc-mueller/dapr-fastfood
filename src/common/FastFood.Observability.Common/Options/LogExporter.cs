namespace FastFood.Observability.Common.Options
{
    public enum LogExporter
    {
        None,
        Console,
        JsonConsole,
        AnsiConsole,
        Otlp,
        OtlpAndConsole,
        OtlpAndJsonConsole,
        OtlpAndAnsiConsole
    }
}