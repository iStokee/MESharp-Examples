using MESharp.Services;
using MESharpExamples.WinForms;

namespace MESharp
{
    /// <summary>
    /// WinForms Example: Demonstrates how to use WinFormsScriptHost for simplified WinForms script setup.
    ///
    /// REQUIREMENTS FOR HOT-RELOAD:
    /// - public static class ScriptEntry in MESharp namespace
    /// - public static void Initialize()
    /// - public static void Shutdown()
    ///
    /// This example uses WinFormsScriptHost which automatically handles:
    /// - WinForms threading (STA thread creation and management)
    /// - Application.Run() lifecycle
    /// - Form lifecycle and shutdown
    /// - Shutdown signal registration with ShutdownMonitor
    ///
    /// WinForms is simpler than WPF and doesn't have the Application.Current singleton issue,
    /// making it a good choice for straightforward UI scripts.
    /// </summary>
    public static class ScriptEntry
    {
        private static readonly UiScriptHostOptions UiOptions = new()
        {
            ScriptName = "MESharp WinForms Example"
        };

        // REQUIRED by ME runtime:
        // - Type name: MESharp.ScriptEntry
        // - Method signature: public static void Initialize()
        public static void Initialize() => WinFormsScriptHost.Run(Main, UiOptions);

        // REQUIRED by ME runtime for clean unload:
        // - Method signature: public static void Shutdown()
        public static void Shutdown() => WinFormsScriptHost.Stop();

        // Script "main" for WinForms: return the root form the host should run.
        private static Form1 Main() => new();
    }
}
