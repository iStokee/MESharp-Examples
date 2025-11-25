# MESharp Examples

This folder contains self-contained script projects that reference `csharp_interop.dll`. They illustrate how to use helper classes like `ScriptRuntimeHost`, `WpfScriptHost`, and `WinFormsScriptHost` to simplify script creation with hot-reload support.

## Projects

| Path | Target | Notes |
| --- | --- | --- |
| `MESharpCLI/` | `net8.0` console | Minimal loop showing `ScriptRuntimeHost` usage with cancellation token support. |
| `MESharpWinForm/` | `net8.0-windows` WinForms | Demonstrates WinForms UI with `WinFormsScriptHost` for simplified setup. |
| `MESharpWPF/` | `net8.0-windows` WPF | Shows WPF UI with `WpfScriptHost` handling Application.Current lifecycle. |
| `MESharpPortables/` | `net8.0-windows` WPF | Modernized Stokee Portables helper demonstrating the Portables API. |
| `StokeeFishing/` | `net8.0-windows` WPF | Full-featured fishing script showing real-world script structure. |
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

All examples follow the hot-reload requirements:

**Absolute Minimum (Required):**
```csharp
namespace MESharp
{
    public static class ScriptEntry
    {
        public static void Initialize() { /* startup code */ }
        public static void Shutdown() { /* cleanup code */ }
    }
}
```

**Using Helper Classes (Recommended):**

These examples use helper classes from `csharp_interop` to simplify setup:

- **ScriptRuntimeHost**: For CLI scripts with async loops and cancellation token support
- **WpfScriptHost**: For WPF scripts with automatic Application.Current lifecycle management
- **WinFormsScriptHost**: For WinForms scripts with automatic form lifecycle management

Example patterns:

```csharp
// CLI script with ScriptRuntimeHost
public static void Initialize() => ScriptRuntimeHost.Run(RunAsync, options);
public static void Shutdown() => ScriptRuntimeHost.Stop();

// WPF script with WpfScriptHost
public static void Initialize() => WpfScriptHost.Run(() => new MainWindow(), options);
public static void Shutdown() => WpfScriptHost.Stop();

// WinForms script with WinFormsScriptHost
public static void Initialize() => WinFormsScriptHost.Run(() => new Form1(), options);
public static void Shutdown() => WinFormsScriptHost.Stop();
```

**Even Simpler Alternatives:**

For even simpler scripts, see the base classes in `csharp_interop/Scripting/`:
- **ScriptBase**: CLI script base class with automatic token management
- **WpfScriptBase**: WPF script base class with automatic Application.Current handling

Or use the minimal templates:
- `cli_template_minimal.txt` - ~80 lines, uses ScriptBase
- `wpf_template_minimal.txt` - ~70 lines, uses WpfScriptBase

## Using These Examples

Use these projects as references when creating new scripts:
1. Clone the example closest to your scenario
2. Rename the assembly and namespace
3. Adjust the UI or background loop for your needs
4. Build and load via ME's "Hot Reload" button

All examples support hot-reload, so you can modify and rebuild without restarting ME!
