using Azure.Identity;
using FastFood.FeatureManagement.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace FastFood.FeatureManagement.Common.Extensions;

/// <summary>
/// Extension methods for configuring feature management with Azure App Configuration support.
/// </summary>
public static class FeatureManagementExtensions
{
    /// <summary>
    /// Adds Azure App Configuration to the configuration builder if configured.
    /// Supports both connection string and managed identity authentication.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <returns>True if Azure App Configuration was configured; otherwise, false.</returns>
    public static bool AddAzureAppConfigurationIfConfigured(this IConfigurationBuilder builder, IConfiguration existingConfig)
    {
        var connectionString = existingConfig["AppConfiguration:ConnectionString"];
        var endpoint = existingConfig["AppConfiguration:Endpoint"];

        if (!string.IsNullOrEmpty(connectionString))
        {
            // Use connection string authentication
            builder.AddAzureAppConfiguration(options =>
            {
                options.Connect(connectionString)
                    .UseFeatureFlags(featureFlagOptions =>
                    {
                        // Reduced cache for faster demo updates (5 seconds instead of 30)
                        // In production, 30-60 seconds is recommended
                        featureFlagOptions.SetRefreshInterval(TimeSpan.FromSeconds(5));
                    });
                    
                // Configure automatic refresh on sentinel key change (optional)
                // This provides near-instant updates when you change flags in Azure Portal
                // Uncomment if you create a "Sentinel" key in Azure App Config:
                // options.ConfigureRefresh(refresh =>
                // {
                //     refresh.Register("Sentinel", refreshAll: true)
                //            .SetCacheExpiration(TimeSpan.FromSeconds(5));
                // });
            });
            return true;
        }
        else if (!string.IsNullOrEmpty(endpoint))
        {
            // Use managed identity authentication
            var credential = new DefaultAzureCredential();
            builder.AddAzureAppConfiguration(options =>
            {
                options.Connect(new Uri(endpoint), credential)
                    .UseFeatureFlags(featureFlagOptions =>
                    {
                        // Reduced cache for faster demo updates (5 seconds instead of 30)
                        featureFlagOptions.SetRefreshInterval(TimeSpan.FromSeconds(5));
                    });
                    
                // Configure automatic refresh on sentinel key change (optional)
                // options.ConfigureRefresh(refresh =>
                // {
                //     refresh.Register("Sentinel", refreshAll: true)
                //            .SetCacheExpiration(TimeSpan.FromSeconds(5));
                // });
            });
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds feature management services with observability support.
    /// Includes built-in feature filters for time windows and percentage-based rollouts.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddObservableFeatureManagement(this IServiceCollection services)
    {
        services.AddFeatureManagement()
            .AddFeatureFilter<Microsoft.FeatureManagement.FeatureFilters.TimeWindowFilter>()
            .AddFeatureFilter<Microsoft.FeatureManagement.FeatureFilters.PercentageFilter>();

        services.AddSingleton<IObservableFeatureManager, ObservableFeatureManager>();

        return services;
    }

    /// <summary>
    /// Checks if Azure App Configuration is configured.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns>True if Azure App Configuration is configured; otherwise, false.</returns>
    public static bool IsAzureAppConfigurationConfigured(this IConfiguration configuration)
    {
        return !string.IsNullOrEmpty(configuration["AppConfiguration:ConnectionString"]) ||
               !string.IsNullOrEmpty(configuration["AppConfiguration:Endpoint"]);
    }
}
