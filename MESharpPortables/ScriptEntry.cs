using MESharp.Services;
using MESharpExamples.Portables;

namespace MESharp
{
    /// <summary>
    /// Portables Example: Demonstrates using the Portables API for portable/node teleportation.
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
    /// The Portables API provides access to teleportation nodes and portable item locations
    /// in RuneScape 3, useful for navigation and teleport scripts.
    /// </summary>
    public static class ScriptEntry
    {
        private static readonly UiScriptHostOptions UiOptions = new()
        {
            ScriptName = "MESharp Portables"
        };

        /// <summary>
        /// Initialize entry point - called by ME's hot-reload system via reflection.
        /// WpfScriptHost will create the window on an STA thread automatically.
        /// </summary>
        public static void Initialize() => WpfScriptHost.Run(() => new MainWindow(), UiOptions);

        /// <summary>
        /// Shutdown entry point - called by ME's hot-reload system via reflection.
        /// WpfScriptHost will close the window and clean up the dispatcher.
        /// </summary>
        public static void Shutdown() => WpfScriptHost.Stop();
    }
}
