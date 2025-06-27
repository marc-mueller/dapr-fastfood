using System.Collections.Immutable;
using FastFood.Observability.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FastFood.Observability.Common
{
    public static class ConfigurationExtensions
    {
        private static ImmutableList<int> _externalPortsCached;
        private static IEnumerable<int> _internalPortsCached;
        private const int DefaultExternalPort = 8080;
        private const int DefaultInternalPort = 8081;
        
        
        public static IEnumerable<int> GetExternalPorts(this IConfiguration configuration, bool useCache = true)
        {
            if (useCache && _externalPortsCached != null)
            {
                return _externalPortsCached;
            }

            var portsString = configuration.GetValue<string>("BaseServiceSettings:ExternalPorts");
            var ports = GetPorts(portsString);
            if (!ports.Any())
            {
               ports.Add(DefaultExternalPort);
            }

            _externalPortsCached = ports.ToImmutableList();
            return ports;
        }

        public static IEnumerable<int> GetInternalPorts(this IConfiguration configuration, bool useCache = true)
        {
            if (useCache && _internalPortsCached != null)
            {
                return _internalPortsCached;
            }

            var portsString = configuration.GetValue<string>("BaseServiceSettings:InternalPorts");
            var ports = GetPorts(portsString);
            if (!ports.Any())
            {
                ports.Add(DefaultInternalPort);
            }
            _internalPortsCached = ports.ToImmutableList();
            return ports;
        }

        private static List<int> GetPorts(string portsValue, bool useCache = true)
        {
            var ports = new List<int>();

            if (!string.IsNullOrEmpty(portsValue))
            {
                var tokens = portsValue.Split(new char[] { ';', ',' });
                foreach (var token in tokens)
                {
                    if (int.TryParse(token, out var port))
                    {
                        ports.Add(port);
                    }
                }
            }

            return ports;
        }

        public static bool IsInternalPort(this IConfiguration configuration, int portToVerify, bool useCache = true)
        {
            return configuration.GetInternalPorts(useCache).Any(p => p == portToVerify);
        }
        
        public static ObservabilityOptions GetObservabilityOptions(this IConfiguration configuration)
        {
            return configuration?.GetSection("Observability").Get<ObservabilityOptions>() ?? new ObservabilityOptions();
        }
        
        public static ILoggerFactory GetBootstrapLoggerFactory(this IConfiguration configuration)
        {
            var bootstrapLoggerFactory = LoggerFactory.Create(
                builder =>
                {
                    builder.ConfigureObservabilityLogging(configuration.GetObservabilityOptions());
                });
            return bootstrapLoggerFactory;
        }
    }
}