# MESharp Examples

This folder contains self-contained script projects that reference `csharp_interop.dll`. They illustrate how to host `ScriptRuntimeHost`, handle logging, and wire UI frameworks (CLI, WinForms, WPF) into MESharp.

## Projects

| Path | Target | Notes |
| --- | --- | --- |
| `MESharpCLI/` | `net8.0` console | Minimal loop that writes to the native logger and shows `ShutdownMonitor` usage. |
| `MESharpWinForm/` | `net8.0-windows` WinForms | Demonstrates UI message loop + `ApplicationConfiguration` bootstrapping. |
| `MESharpWPF/` | `net8.0-windows` WPF | Mirrors the pattern Orbit uses for script UIs, including dispatcher shutdown. |
| `MESharpExamples.sln` | Solution for all samples | Load this in Visual Studio to build/debug each project. |

## Prerequisites

- .NET SDK 8.0 with Windows desktop workloads
- `csharp_interop.dll` built from `C#/csharp_interop`
- Visual Studio 2022 (recommended) or `dotnet` CLI

## Building

```powershell
dotnet build MESharp-Examples/MESharpExamples.sln -c Debug
```

Each project copies its output to `%USERPROFILE%\MemoryError\MESharpExamples\<Framework>\` after build so the injector/orbit host can pick up the DLL easily.

## Script Entry Pattern

All examples expose the standard entry points expected by MemoryError:

```csharp
public static class ScriptEntry
{
    public static void Initialize() => ScriptRuntimeHost.Run(RunAsync, HostOptions);
    public static void Shutdown() => ScriptRuntimeHost.Stop();
    public static void SetLogger(IntPtr logger) => ScriptRuntimeHost.SetLogger(logger);
}
```

That lets the native host drive managed lifetimes uniformly while still allowing each sample to plug in its own UI or background loop.

Use these projects as references when creating new scripts—clone one closest to your scenario, rename the assembly, and adjust the run loop/UI.
