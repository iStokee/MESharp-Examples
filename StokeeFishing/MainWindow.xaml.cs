using System.Windows;
using StokeeFishing.ViewModels;

namespace StokeeFishing;

/// <summary>
/// Main window for the Stokee AIO Fishing script.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Start the update timer when window loads
        _viewModel.StartUpdateTimer();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        // Clean up when window closes
        _viewModel.StopUpdateTimer();
        _viewModel.Stop();
    }
}
