using System;
using System.Runtime.InteropServices;
using MESharp.Services;

namespace MESharpExamples.WinForms
{
    /// <summary>
    /// Thin wrapper that delegtes all MESharp entry points to <see cref="WinFormsScriptHost"/>.
    /// </summary>
    public static class ScriptEntry
    {
        private static readonly UiScriptHostOptions UiOptions = new()
        {
            ScriptName = "MESharp WinForms Example"
        };

        public static void Initialize() => WinFormsScriptHost.Run(() => new Form1(), UiOptions);

        [UnmanagedCallersOnly]
        public static void Initialize_Native() => WinFormsScriptHost.Run(() => new Form1(), UiOptions);

        public static void Shutdown() => WinFormsScriptHost.Stop();

        [UnmanagedCallersOnly]
        public static void Shutdown_Native() => WinFormsScriptHost.Stop();

        [UnmanagedCallersOnly]
        public static void SetLogger(IntPtr loggerCallbackPtr) => ScriptRuntimeHost.SetLogger(loggerCallbackPtr);
    }
}
