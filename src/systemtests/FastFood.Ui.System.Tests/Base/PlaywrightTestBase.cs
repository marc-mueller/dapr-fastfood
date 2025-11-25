using System.Collections.Concurrent;
using FastFood.Ui.System.Tests.Configuration;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

namespace FastFood.Ui.System.Tests.Base;

/// <summary>
/// Base class for Playwright tests using official Microsoft.Playwright.Xunit.v3 package
/// Extends PageTest to provide:
/// - Configuration from environment variables
/// - Video recording with meaningful names (test name + tab name)
/// - Azure DevOps test artifact integration
/// 
/// BEST PRACTICE: Tests create page objects directly using BrowserHelper.
/// The PageTest base class automatically handles test name tracking and video recording.
/// </summary>
public abstract class PlaywrightTestBase : PageTest
{
    private static ConcurrentDictionary<string,string> _kvStorage = new ConcurrentDictionary<string, string>();
    
    protected TestConfiguration Configuration { get; }
    protected string VideoBaseDirectory { get; }
    
    private string? _currentTestName;

    protected PlaywrightTestBase()
    {
        Configuration = new TestConfiguration();
        var testRunTimestamp = _kvStorage.GetOrAdd($"TestRunTimestamp{TestContext.Current.TestCollection.UniqueID}", DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
        VideoBaseDirectory = Path.Combine(Configuration.Video.Directory, testRunTimestamp);
        
        if (Configuration.Video.Enabled)
        {
            Directory.CreateDirectory(VideoBaseDirectory);
        }
    }
    
    /// <summary>
    /// Get the current test method name
    /// </summary>
    private string GetCurrentTestName()
    {
        if (_currentTestName == null)
        {
            var testContext = Xunit.TestContext.Current;
            
            // Use reflection to get the test name from TestContext
            // In xUnit v3, the test information is available through the TestContext
            var testProperty = testContext.GetType().GetProperty("Test");
            if (testProperty != null)
            {
                var test = testProperty.GetValue(testContext);
                if (test != null)
                {
                    var testNameProperty = test.GetType().GetProperty("TestDisplayName") 
                        ?? test.GetType().GetProperty("DisplayName")
                        ?? test.GetType().GetProperty("Name");
                    
                    if (testNameProperty != null)
                    {
                        var fullName = testNameProperty.GetValue(test)?.ToString() ?? "";
                        var lastDot = fullName.LastIndexOf('.');
                        _currentTestName = lastDot >= 0 ? fullName.Substring(lastDot + 1) : fullName;
                        return _currentTestName;
                    }
                }
            }
            
            // Fallback: use the class name if we can't get test name
            _currentTestName = GetType().Name;
        }
        return _currentTestName;
    }

    /// <summary>
    /// Override to configure browser context options including video recording
    /// </summary>
    public override BrowserNewContextOptions ContextOptions()
    {
        var testName = GetCurrentTestName();
        
        // Create test-specific video directory
        var videoDir = Path.Combine(VideoBaseDirectory, testName);
        
        if (Configuration.Video.Enabled)
        {
            Directory.CreateDirectory(videoDir);
        }
        
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            RecordVideoDir = Configuration.Video.Enabled ? videoDir : null,
            RecordVideoSize = new RecordVideoSize 
            { 
                Width = Configuration.Video.Width, 
                Height = Configuration.Video.Height 
            },
            ViewportSize = new ViewportSize 
            { 
                Width = Configuration.Video.Width, 
                Height = Configuration.Video.Height 
            },
            BaseURL = null // We handle URLs in page objects
        };
    }

    /// <summary>
    /// Override DisposeAsync to handle video cleanup
    /// </summary>
    public override async ValueTask DisposeAsync()
    {
        await RenameAndUploadVideosAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Rename videos with meaningful names
    /// PageTest automatically creates videos with GUIDs, we rename them to include test name and tab name
    /// </summary>
    private async Task RenameAndUploadVideosAsync()
    {
        if (!Configuration.Video.Enabled)
        {
            return;
        }
        

        var testName = GetCurrentTestName();
        var videoDir = Path.Combine(VideoBaseDirectory, testName);
        var allVideos = Directory.GetFiles(videoDir, "*.webm", SearchOption.TopDirectoryOnly);

        foreach (var video in allVideos)
        {
            if (TestContext.Current.KeyValueStorage.TryGetValue(Path.GetFileName(video), out object? newFilenameObj) && newFilenameObj != null)
            {
                var newFileName = newFilenameObj.ToString() ?? "unknown.webm";
                var newPath = Path.Combine(videoDir, newFileName);
                
                // Rename the file if needed
                if (video != newPath)
                {
                    // Use File.Move with overwrite to ensure clean rename
                    if (File.Exists(newPath))
                    {
                        File.Delete(newPath);
                    }
                    File.Move(video, newPath);
                }
            }
        }
    }
}
