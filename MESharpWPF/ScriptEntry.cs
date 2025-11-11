using System;
using System.Runtime.InteropServices;
using MESharp.Services;

namespace MESharpExamples.WPF
{
    /// <summary>
    /// Delegates all MESharp entry points to the shared <see cref="WpfScriptHost"/>.
    /// </summary>
    public static class ScriptEntry
    {
        private static readonly UiScriptHostOptions UiOptions = new()
        {
            ScriptName = "MESharp WPF Example"
        };

        public static void Initialize() => WpfScriptHost.Run(() => new MainWindow(), UiOptions);

        [UnmanagedCallersOnly]
        public static void Initialize_Native() => WpfScriptHost.Run(() => new MainWindow(), UiOptions);

        public static void Shutdown() => WpfScriptHost.Stop();

        [UnmanagedCallersOnly]
        public static void Shutdown_Native() => WpfScriptHost.Stop();

        [UnmanagedCallersOnly]
        public static void SetLogger(IntPtr loggerCallbackPtr) => ScriptRuntimeHost.SetLogger(loggerCallbackPtr);
    }
}
