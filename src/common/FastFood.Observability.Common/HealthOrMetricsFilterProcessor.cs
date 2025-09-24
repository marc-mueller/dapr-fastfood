using System.Diagnostics;
using OpenTelemetry;

namespace FastFood.Observability.Common
{
    internal sealed class HealthOrMetricsFilterProcessor : BaseProcessor<Activity>
    {
        public override void OnEnd(Activity activity)
        {
            if (IsHealthOrMetricsEndpoint(activity))
            {
                activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
            }
        }
    
        private static bool IsHealthOrMetricsEndpoint(Activity activity)
        {
            if (activity == null)
            {
                return false;
            }

            // Prefer semantic-convention tags over DisplayName
            // HTTP paths
            var route = (activity.GetTagItem("http.route") as string)
                        ?? (activity.GetTagItem("url.path") as string)
                        ?? (activity.GetTagItem("http.target") as string);

            var path = (route ?? activity.DisplayName) ?? string.Empty;
            path = path.ToLowerInvariant();

            // Common noisy endpoints (HTTP)
            if (path.StartsWith("/health") ||
                path.StartsWith("/healthz") ||
                path.StartsWith("/ready") ||
                path.StartsWith("/health/readiness") ||
                path.StartsWith("/readiness") ||
                path.StartsWith("/live") ||
                path.StartsWith("/liveness") ||
                path.StartsWith("/startup") ||
                path.StartsWith("/health/startup") ||
                path.StartsWith("/metrics"))
            {
                return true;
            }

            // gRPC health checks (rpc.system = grpc, service rpc.health or grpc.health.v1.Health)
            var rpcSystem = activity.GetTagItem("rpc.system") as string;
            if (string.Equals(rpcSystem, "grpc", StringComparison.OrdinalIgnoreCase))
            {
                var rpcService = activity.GetTagItem("rpc.service") as string;
                var rpcMethod = activity.GetTagItem("rpc.method") as string;

                if (!string.IsNullOrEmpty(rpcService))
                {
                    var svc = rpcService.ToLowerInvariant();
                    if (svc == "grpc.health.v1.health" || svc.EndsWith(".health"))
                    {
                        return true;
                    }
                }

                if (!string.IsNullOrEmpty(rpcMethod))
                {
                    var method = rpcMethod.ToLowerInvariant();
                    if (method == "check" || method == "watch")
                    {
                        return true;
                    }
                }
            }

            // Prometheus scrape based on user-agent (best-effort; tag names vary by instrumentation version)
            var ua = (activity.GetTagItem("user_agent.original") as string)
                     ?? (activity.GetTagItem("http.user_agent") as string)
                     ?? (activity.GetTagItem("http.request.header.user-agent") as string);
            if (!string.IsNullOrEmpty(ua) && ua.Contains("Prometheus", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}