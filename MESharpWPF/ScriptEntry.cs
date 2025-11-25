using MESharp.Services;
using MESharpExamples.WPF;

namespace MESharp
{
    /// <summary>
    /// WPF Example: Demonstrates how to use WpfScriptHost for simplified WPF script setup.
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
    /// For even simpler WPF scripts, see WpfScriptBase in csharp_interop/Scripting/
    /// or use the wpf_template_minimal.txt template.
    /// </summary>
    public static class ScriptEntry
    {
        private static readonly UiScriptHostOptions UiOptions = new()
        {
            ScriptName = "MESharp WPF Example"
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
