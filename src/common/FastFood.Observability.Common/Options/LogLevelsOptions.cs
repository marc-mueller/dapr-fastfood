using Microsoft.Extensions.Logging;

namespace FastFood.Observability.Common.Options
{
    public class LogLevelsOptions
    {
        public LogLevel Default { get; set; }
        public Dictionary<string, LogLevel> Filters { get; set; }
    }
}