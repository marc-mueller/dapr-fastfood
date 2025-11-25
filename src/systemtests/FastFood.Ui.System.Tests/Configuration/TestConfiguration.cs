namespace FastFood.Ui.System.Tests.Configuration;

/// <summary>
/// Configuration for UI system tests
/// Reads from environment variables with fallback to defaults
/// </summary>
public class TestConfiguration
{
    /// <summary>
    /// Self Service POS URL
    /// Environment variable: TEST_POS_URL
    /// </summary>
    public string SelfServicePosUrl { get; set; }

    /// <summary>
    /// Kitchen Monitor URL
    /// Environment variable: TEST_KITCHEN_URL
    /// </summary>
    public string KitchenMonitorUrl { get; set; }

    /// <summary>
    /// Customer Order Status URL
    /// Environment variable: TEST_ORDERSTATUS_URL
    /// </summary>
    public string CustomerOrderStatusUrl { get; set; }

    /// <summary>
    /// Video recording settings
    /// </summary>
    public VideoSettings Video { get; set; }

    /// <summary>
    /// Timeout settings in milliseconds
    /// </summary>
    public TimeoutSettings Timeouts { get; set; }

    public TestConfiguration()
    {
        // Read URLs from environment with fallbacks
        SelfServicePosUrl = Environment.GetEnvironmentVariable("TEST_POS_URL") ?? "https://pos.localtest.me/";
        KitchenMonitorUrl = Environment.GetEnvironmentVariable("TEST_KITCHEN_URL") ?? "https://kitchen.localtest.me/";
        CustomerOrderStatusUrl = Environment.GetEnvironmentVariable("TEST_ORDERSTATUS_URL") ?? "https://orderstatus.localtest.me/";
        
        // Initialize settings objects
        Video = new VideoSettings();
        Timeouts = new TimeoutSettings();
    }
}

public class VideoSettings
{
    /// <summary>
    /// Enable video recording
    /// Environment variable: VIDEO_ENABLED (default: true)
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Video directory path
    /// Environment variable: VIDEO_OUTPUT_DIR (default: videos)
    /// </summary>
    public string Directory { get; set; }

    /// <summary>
    /// Target video width in pixels (used for RecordVideoSize and viewport)
    /// Environment variable: VIDEO_WIDTH (default: 1920)
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Target video height in pixels (used for RecordVideoSize and viewport)
    /// Environment variable: VIDEO_HEIGHT (default: 1080)
    /// </summary>
    public int Height { get; set; }

    public VideoSettings()
    {
        // Read video enabled flag
        var videoEnabledEnv = Environment.GetEnvironmentVariable("VIDEO_ENABLED");
        Enabled = string.IsNullOrEmpty(videoEnabledEnv) || 
                  string.Equals(videoEnabledEnv, "true", StringComparison.OrdinalIgnoreCase);
        
        // Read video directory - prefer VIDEO_OUTPUT_DIR from pipeline
        Directory = Environment.GetEnvironmentVariable("VIDEO_OUTPUT_DIR") ?? "videos";

        // Read width/height if provided, otherwise default to 1920x1080
        var widthEnv = Environment.GetEnvironmentVariable("VIDEO_WIDTH");
        var heightEnv = Environment.GetEnvironmentVariable("VIDEO_HEIGHT");

        if (!int.TryParse(widthEnv, out var parsedWidth))
        {
            parsedWidth = 1920;
        }

        if (!int.TryParse(heightEnv, out var parsedHeight))
        {
            parsedHeight = 1080;
        }

        Width = parsedWidth;
        Height = parsedHeight;
    }
}

public class TimeoutSettings
{
    /// <summary>
    /// Default timeout for actions (ms)
    /// </summary>
    public int DefaultTimeout { get; set; } = 30000;

    /// <summary>
    /// Timeout for navigation (ms)
    /// </summary>
    public int NavigationTimeout { get; set; } = 60000;

    /// <summary>
    /// Timeout for order processing (ms)
    /// </summary>
    public int OrderProcessingTimeout { get; set; } = 120000;
}
