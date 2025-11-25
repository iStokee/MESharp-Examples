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

        /// <summary>
        /// Initialize entry point - called by ME's hot-reload system via reflection.
        /// WinFormsScriptHost will create the form on an STA thread automatically.
        /// </summary>
        public static void Initialize() => WinFormsScriptHost.Run(() => new Form1(), UiOptions);

        /// <summary>
        /// Shutdown entry point - called by ME's hot-reload system via reflection.
        /// WinFormsScriptHost will close the form and clean up the thread.
        /// </summary>
        public static void Shutdown() => WinFormsScriptHost.Stop();
    }
}
