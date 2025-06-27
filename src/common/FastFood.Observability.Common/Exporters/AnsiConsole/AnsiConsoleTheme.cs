namespace FastFood.Observability.Common.Exporters.AnsiConsole
{
    /// <summary>
    /// ANSI console colors used for log output
    /// </summary>
    public class AnsiConsoleTheme
    {
        private const string AnsiReset = "\x1b[0m";
        private const string AnsiBlack = "\x1b[30m";
        private const string AnsiRed = "\x1b[31m";
        private const string AnsiGreen = "\x1b[32m";
        private const string AnsiYellow = "\x1b[33m";
        private const string AnsiBlue = "\x1b[34m";
        private const string AnsiBrightBlue = "\x1b[94m";
        private const string AnsiMagenta = "\x1b[35m";
        private const string AnsiCyan = "\x1b[36m";
        private const string AnsiWhite = "\x1b[37m";
        private const string AnsiBrightRed = "\x1b[91m";
        private const string AnsiBrightGreen = "\x1b[92m";
        private const string AnsiBrightYellow = "\x1b[93m";
        private const string AnsiBrightMagenta = "\x1b[95m";
        private const string AnsiBrightCyan = "\x1b[96m";
        private const string AnsiBrightWhite = "\x1b[97m";
        
        private readonly Dictionary<LogFormatElement, string> _styles;

        /// <summary>
        /// The Literate theme, inspired by Serilog's literate console theme
        /// </summary>
        public static AnsiConsoleTheme Literate { get; } = new AnsiConsoleTheme(
            new Dictionary<LogFormatElement, string>
            {
                [LogFormatElement.Text] = AnsiReset,
                [LogFormatElement.SecondaryText] = AnsiBrightBlack,
                [LogFormatElement.TertiaryText] = AnsiBrightBlack,
                [LogFormatElement.Timestamp] = AnsiReset,
                [LogFormatElement.LogLevel] = AnsiReset,
                [LogFormatElement.LogLevelTrace] = AnsiBrightBlack,
                [LogFormatElement.LogLevelDebug] = AnsiBrightBlue,
                [LogFormatElement.LogLevelInformation] = AnsiBrightGreen,
                [LogFormatElement.LogLevelWarning] = AnsiBrightYellow,
                [LogFormatElement.LogLevelError] = AnsiBrightRed,
                [LogFormatElement.LogLevelCritical] = AnsiMagenta,
                [LogFormatElement.Exception] = AnsiBrightRed,
                [LogFormatElement.TraceId] = AnsiBrightBlue,
                [LogFormatElement.SpanId] = AnsiBrightBlue,
                [LogFormatElement.AttributeKey] = AnsiBrightCyan,
                [LogFormatElement.AttributeValue] = AnsiCyan
            });

        /// <summary>
        /// A theme with colors suitable for dark console backgrounds (default)
        /// </summary>
        public static AnsiConsoleTheme Code { get; } = new AnsiConsoleTheme(
            new Dictionary<LogFormatElement, string>
            {
                [LogFormatElement.Text] = AnsiReset,
                [LogFormatElement.SecondaryText] = AnsiGreen,
                [LogFormatElement.TertiaryText] = AnsiBlue,
                [LogFormatElement.Timestamp] = AnsiGreen,
                [LogFormatElement.LogLevel] = AnsiReset,
                [LogFormatElement.LogLevelTrace] = AnsiMagenta,
                [LogFormatElement.LogLevelDebug] = AnsiBrightCyan,
                [LogFormatElement.LogLevelInformation] = AnsiBrightWhite,
                [LogFormatElement.LogLevelWarning] = AnsiBrightYellow,
                [LogFormatElement.LogLevelError] = AnsiBrightRed,
                [LogFormatElement.LogLevelCritical] = AnsiBrightWhite + "\x1b[41m", // White on red background
                [LogFormatElement.Exception] = AnsiBrightRed,
                [LogFormatElement.TraceId] = AnsiCyan,
                [LogFormatElement.SpanId] = AnsiCyan,
                [LogFormatElement.AttributeKey] = AnsiBrightYellow,
                [LogFormatElement.AttributeValue] = AnsiYellow
            });

        private const string AnsiBrightBlack = "\x1b[90m";

        private AnsiConsoleTheme(Dictionary<LogFormatElement, string> styles)
        {
            _styles = styles ?? throw new ArgumentNullException(nameof(styles));
        }

        /// <summary>
        /// Get the ANSI color code for a given log element
        /// </summary>
        public string GetStyle(LogFormatElement element)
        {
            if (_styles.TryGetValue(element, out var style))
            {
                return style;
            }

            return AnsiReset;
        }

        /// <summary>
        /// Reset ANSI color codes
        /// </summary>
        public string Reset => AnsiReset;
    }

    /// <summary>
    /// Elements in a log output that can be styled
    /// </summary>
    public enum LogFormatElement
    {
        Text,
        SecondaryText,
        TertiaryText,
        Timestamp,
        LogLevel,
        LogLevelTrace,
        LogLevelDebug,
        LogLevelInformation,
        LogLevelWarning,
        LogLevelError,
        LogLevelCritical,
        Exception,
        TraceId,
        SpanId,
        AttributeKey,
        AttributeValue
    }
}