using MESharp.Services;

namespace MESharp;

/// <summary>
/// Stokee AIO Fishing: A complete fishing script example demonstrating real-world script structure.
///
/// REQUIREMENTS FOR HOT-RELOAD:
/// - public static class ScriptEntry in MESharp namespace
/// - public static void Initialize()
/// - public static void Shutdown()
///
/// This example uses WpfScriptHost which automatically handles:
/// - WPF threading (STA thread creation and management)
/// - Application.Current lifecycle (reused across hot-reloads)
/// - Dispatcher lifecycle and shutdown
/// - Shutdown signal registration with ShutdownMonitor
///
/// This is a full-featured example showing:
/// - Complex script state management
/// - Multiple fishing spots and methods
/// - User configuration UI
/// - Anti-ban features
/// - Progress tracking and statistics
/// </summary>
public static class ScriptEntry
{
    private static readonly UiScriptHostOptions UiOptions = new()
    {
        ScriptName = "Stokee AIO Fishing"
    };

    /// <summary>
    /// Initialize entry point - called by ME's hot-reload system via reflection.
    /// WpfScriptHost will create the window on an STA thread automatically.
    /// </summary>
    public static void Initialize()
        => WpfScriptHost.Run(() => new StokeeFishing.MainWindow(), UiOptions);

    /// <summary>
    /// Shutdown entry point - called by ME's hot-reload system via reflection.
    /// WpfScriptHost will close the window and clean up the dispatcher.
    /// </summary>
    public static void Shutdown()
        => WpfScriptHost.Stop();
}
