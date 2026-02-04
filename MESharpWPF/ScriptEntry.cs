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

        // REQUIRED by ME runtime:
        // - Type name: MESharp.ScriptEntry
        // - Method signature: public static void Initialize()
        public static void Initialize() => WpfScriptHost.Run(Main, UiOptions);

        // REQUIRED by ME runtime for clean unload:
        // - Method signature: public static void Shutdown()
        public static void Shutdown() => WpfScriptHost.Stop();

        // Script "main" for WPF: return the root window the host should run.
        private static MainWindow Main() => new();
    }
}
