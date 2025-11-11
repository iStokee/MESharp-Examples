using System.Windows.Forms;

namespace MESharpWinForm;

/// <summary>
/// Provides central configuration for WinForms bootstrapping, mirroring the default .NET templates.
/// </summary>
internal static partial class ApplicationConfiguration
{
    public static void Initialize()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
    }
}
