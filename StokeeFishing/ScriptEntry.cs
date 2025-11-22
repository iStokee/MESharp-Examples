using System;
using System.Runtime.InteropServices;
using MESharp.Services;

namespace MESharp;

/// <summary>
/// Entry point for the Stokee AIO Fishing script.
/// Delegates all MESharp entry points to the shared WpfScriptHost.
/// </summary>
public static class ScriptEntry
{
    private static readonly UiScriptHostOptions UiOptions = new()
    {
        ScriptName = "Stokee AIO Fishing"
    };

    public static void Initialize()
        => WpfScriptHost.Run(() => new StokeeFishing.MainWindow(), UiOptions);

    [UnmanagedCallersOnly]
    public static void Initialize_Native()
        => WpfScriptHost.Run(() => new StokeeFishing.MainWindow(), UiOptions);

    public static void Shutdown()
        => WpfScriptHost.Stop();

    [UnmanagedCallersOnly]
    public static void Shutdown_Native()
        => WpfScriptHost.Stop();

    [UnmanagedCallersOnly]
    public static void SetLogger(IntPtr loggerCallbackPtr)
        => ScriptRuntimeHost.SetLogger(loggerCallbackPtr);
}
