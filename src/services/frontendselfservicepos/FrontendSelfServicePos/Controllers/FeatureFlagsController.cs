using FastFood.FeatureManagement.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

namespace FrontendSelfServicePos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeatureFlagsController : ControllerBase
{
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<FeatureFlagsController> _logger;

    public FeatureFlagsController(IFeatureManager featureManager, ILogger<FeatureFlagsController> logger)
    {
        _featureManager = featureManager;
        _logger = logger;
    }

    /// <summary>
    /// Get current feature flag states for the frontend.
    /// Supports user context for percentage-based and targeting filters.
    /// </summary>
    /// <param name="userId">Required user ID for targeting filters (should be session-based)</param>
    /// <returns>Dictionary of feature flags with their current state</returns>
    [HttpGet]
    public async Task<IActionResult> GetActiveFeatures([FromQuery] string? userId = null)
    {
        try
        {
            // Generate a default userId if not provided (fallback for backwards compatibility)
            userId ??= Guid.NewGuid().ToString();

            // Create targeting context for percentage/targeting filters
            var targetingContext = new TargetingContext
            {
                UserId = userId,
                Groups = new List<string>() // Could add user groups here for targeting
            };

            var flags = new Dictionary<string, bool>
            {
                [FeatureFlags.LoyaltyProgram] = await _featureManager.IsEnabledAsync(FeatureFlags.LoyaltyProgram, targetingContext),
                [FeatureFlags.NewCheckoutExperience] = await _featureManager.IsEnabledAsync(FeatureFlags.NewCheckoutExperience, targetingContext),
                [FeatureFlags.DarkMode] = await _featureManager.IsEnabledAsync(FeatureFlags.DarkMode, targetingContext)
            };

            // Add cache headers to prevent browser caching - we want fresh flag values
            Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Append("Pragma", "no-cache");
            Response.Headers.Append("Expires", "0");

            _logger.LogDebug("Feature flags evaluated for userId: {UserId} - Loyalty: {Loyalty}, Checkout: {Checkout}, DarkMode: {DarkMode}",
                userId, flags[FeatureFlags.LoyaltyProgram], flags[FeatureFlags.NewCheckoutExperience], flags[FeatureFlags.DarkMode]);

            return Ok(flags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve feature flags");
            return StatusCode(500, "Failed to retrieve feature flags");
        }
    }
}
